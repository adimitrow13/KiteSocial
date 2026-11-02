import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/api_constants.dart';
import '../models/models.dart';

class ApiService {
  static final ApiService _instance = ApiService._internal();
  factory ApiService() => _instance;
  ApiService._internal();

  String? _authToken;
  User? _currentUser;

  User? get currentUser => _currentUser;
  bool get isAuthenticated => _authToken != null;

  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    _authToken = prefs.getString('auth_token');
    if (_authToken != null) {
      await fetchCurrentUser();
    }
  }

  Map<String, String> _headers({bool requiresAuth = true}) {
    final headers = <String, String>{
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };
    if (requiresAuth && _authToken != null) {
      headers['Authorization'] = 'Bearer $_authToken';
    }
    return headers;
  }

  // --- AUTHENTICATION ---
  Future<bool> login(String usernameOrEmail, String password) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.login}');
    final response = await http.post(
      url,
      headers: _headers(requiresAuth: false),
      body: jsonEncode({'usernameOrEmail': usernameOrEmail, 'password': password}),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      _authToken = data['token'];
      _currentUser = User.fromJson(data['user']);
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('auth_token', _authToken!);
      return true;
    } else if (response.statusCode == 423) {
      throw Exception('Акаунтът е временно заключен поради твърде много неуспешни опити.');
    } else if (response.statusCode == 429) {
      throw Exception('Твърде много опити. Моля изчакайте минута преди нов опит.');
    } else {
      final err = jsonDecode(response.body);
      throw Exception(err['message'] ?? 'Невалидно потребителско име или парола.');
    }
  }

  Future<bool> register({
    required String email,
    required String username,
    required String password,
    required String fullName,
    int skillLevel = 1,
  }) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.register}');
    final response = await http.post(
      url,
      headers: _headers(requiresAuth: false),
      body: jsonEncode({
        'email': email,
        'username': username,
        'password': password,
        'fullName': fullName,
        'skillLevel': skillLevel,
      }),
    );

    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      _authToken = data['token'];
      _currentUser = User.fromJson(data['user']);
      final prefs = await SharedPreferences.getInstance();
      await prefs.setString('auth_token', _authToken!);
      return true;
    } else {
      final err = jsonDecode(response.body);
      throw Exception(err['message'] ?? 'Грешка при регистрация.');
    }
  }

  Future<void> logout() async {
    _authToken = null;
    _currentUser = null;
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove('auth_token');
  }

  Future<User?> fetchCurrentUser() async {
    if (_authToken == null) return null;
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.currentUser}');
    try {
      final response = await http.get(url, headers: _headers());
      if (response.statusCode == 200) {
        _currentUser = User.fromJson(jsonDecode(response.body));
        return _currentUser;
      }
    } catch (_) {}
    return null;
  }

  // --- SPOTS ---
  Future<List<Spot>> getSpots({String? country, String? search}) async {
    var uriString = '${ApiConstants.baseUrl}${ApiConstants.spots}';
    final queryParams = <String, String>{};
    if (country != null && country.isNotEmpty) queryParams['country'] = country;
    if (search != null && search.isNotEmpty) queryParams['search'] = search;

    final uri = Uri.parse(uriString).replace(queryParameters: queryParams.isEmpty ? null : queryParams);
    final response = await http.get(uri, headers: _headers(requiresAuth: false));

    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((item) => Spot.fromJson(item)).toList();
    }
    throw Exception('Грешка при зареждане на спотовете.');
  }

  Future<Spot> createSpot({
    required String name,
    required String country,
    String? region,
    double? latitude,
    double? longitude,
    int waterType = 1,
    String? bestWindDirections,
    String? description,
  }) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.spots}');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({
        'name': name,
        'country': country,
        'region': region,
        'latitude': latitude,
        'longitude': longitude,
        'waterType': waterType,
        'bestWindDirections': bestWindDirections,
        'description': description,
      }),
    );

    if (response.statusCode == 201) {
      return Spot.fromJson(jsonDecode(response.body));
    }
    throw Exception('Неуспешно добавяне на спот.');
  }

  // --- SESSIONS ---
  Future<List<KiteSession>> getMySessions() async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.mySessions}');
    final response = await http.get(url, headers: _headers());

    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((item) => KiteSession.fromJson(item)).toList();
    }
    throw Exception('Грешка при зареждане на сесиите.');
  }

  Future<Map<String, dynamic>> getSessionStats() async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.sessionStats}');
    final response = await http.get(url, headers: _headers());

    if (response.statusCode == 200) {
      return jsonDecode(response.body);
    }
    return {'totalSessions': 0, 'totalMinutesOnWater': 0, 'maxJumpHeightRecord': 0.0};
  }

  Future<KiteSession> logSession({
    required String spotId,
    required DateTime sessionDate,
    required int durationMinutes,
    required double windSpeedKnots,
    required double kiteSize,
    required int boardType,
    double? maxJumpHeightMeters,
    String? notes,
  }) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.sessions}');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({
        'spotId': spotId,
        'sessionDate': sessionDate.toIso8601String(),
        'durationMinutes': durationMinutes,
        'windSpeedKnots': windSpeedKnots,
        'kiteSize': kiteSize,
        'boardType': boardType,
        'maxJumpHeightMeters': maxJumpHeightMeters,
        'notes': notes,
      }),
    );

    if (response.statusCode == 201) {
      return KiteSession.fromJson(jsonDecode(response.body));
    }
    throw Exception('Грешка при записване на сесията.');
  }

  // --- POSTS / FEED ---
  Future<List<Post>> getFeed({int page = 1, int pageSize = 20}) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.posts}?page=$page&pageSize=$pageSize');
    final response = await http.get(url, headers: _headers(requiresAuth: false));

    if (response.statusCode == 200) {
      final List data = jsonDecode(response.body);
      return data.map((item) => Post.fromJson(item)).toList();
    }
    throw Exception('Грешка при зареждане на фийда.');
  }

  Future<Post> createPost({
    required String content,
    String? spotId,
    String? kiteSessionId,
    String? mediaUrl,
  }) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.posts}');
    final response = await http.post(
      url,
      headers: _headers(),
      body: jsonEncode({
        'content': content,
        'spotId': spotId,
        'kiteSessionId': kiteSessionId,
        'mediaUrl': mediaUrl,
      }),
    );

    if (response.statusCode == 201) {
      return Post.fromJson(jsonDecode(response.body));
    }
    throw Exception('Грешка при публикуване.');
  }

  Future<bool> toggleLike(String postId) async {
    final url = Uri.parse('${ApiConstants.baseUrl}${ApiConstants.posts}/$postId/like');
    final response = await http.post(url, headers: _headers());
    if (response.statusCode == 200) {
      final data = jsonDecode(response.body);
      return data['isLiked'] ?? false;
    }
    return false;
  }
}
