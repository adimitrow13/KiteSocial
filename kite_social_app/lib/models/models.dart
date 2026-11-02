class User {
  final String id;
  final String username;
  final String email;
  final String fullName;
  final int skillLevel;
  final String? bio;
  final String? avatarUrl;

  User({
    required this.id,
    required this.username,
    required this.email,
    required this.fullName,
    required this.skillLevel,
    this.bio,
    this.avatarUrl,
  });

  factory User.fromJson(Map<String, dynamic> json) {
    return User(
      id: json['id'] ?? '',
      username: json['username'] ?? '',
      email: json['email'] ?? '',
      fullName: json['fullName'] ?? '',
      skillLevel: json['skillLevel'] ?? 1,
      bio: json['bio'],
      avatarUrl: json['avatarUrl'],
    );
  }
}

class Spot {
  final String id;
  final String name;
  final String country;
  final String? region;
  final double? latitude;
  final double? longitude;
  final int waterType; // 1: Flat, 2: Chop, 3: Waves
  final String? bestWindDirections;
  final String? description;
  final String? imageUrl;
  final int totalSessionsLogged;

  Spot({
    required this.id,
    required this.name,
    required this.country,
    this.region,
    this.latitude,
    this.longitude,
    required this.waterType,
    this.bestWindDirections,
    this.description,
    this.imageUrl,
    this.totalSessionsLogged = 0,
  });

  factory Spot.fromJson(Map<String, dynamic> json) {
    return Spot(
      id: json['id'] ?? '',
      name: json['name'] ?? '',
      country: json['country'] ?? '',
      region: json['region'],
      latitude: (json['latitude'] as num?)?.toDouble(),
      longitude: (json['longitude'] as num?)?.toDouble(),
      waterType: json['waterType'] ?? 1,
      bestWindDirections: json['bestWindDirections'],
      description: json['description'],
      imageUrl: json['imageUrl'],
      totalSessionsLogged: json['totalSessionsLogged'] ?? 0,
    );
  }

  String get waterTypeDisplay {
    switch (waterType) {
      case 1:
        return 'Flat';
      case 2:
        return 'Chop';
      case 3:
        return 'Waves';
      default:
        return 'Mixed';
    }
  }
}

class KiteSession {
  final String id;
  final String spotId;
  final String spotName;
  final DateTime sessionDate;
  final int durationMinutes;
  final double windSpeedKnots;
  final double kiteSize;
  final int boardType; // 1: TwinTip, 2: Surfboard, 3: Foil
  final double? maxJumpHeightMeters;
  final String? notes;

  KiteSession({
    required this.id,
    required this.spotId,
    required this.spotName,
    required this.sessionDate,
    required this.durationMinutes,
    required this.windSpeedKnots,
    required this.kiteSize,
    required this.boardType,
    this.maxJumpHeightMeters,
    this.notes,
  });

  factory KiteSession.fromJson(Map<String, dynamic> json) {
    return KiteSession(
      id: json['id'] ?? '',
      spotId: json['spotId'] ?? '',
      spotName: json['spotName'] ?? 'Unknown Spot',
      sessionDate: DateTime.tryParse(json['sessionDate'] ?? '') ?? DateTime.now(),
      durationMinutes: json['durationMinutes'] ?? 0,
      windSpeedKnots: (json['windSpeedKnots'] as num?)?.toDouble() ?? 0.0,
      kiteSize: (json['kiteSize'] as num?)?.toDouble() ?? 0.0,
      boardType: json['boardType'] ?? 1,
      maxJumpHeightMeters: (json['maxJumpHeightMeters'] as num?)?.toDouble(),
      notes: json['notes'],
    );
  }
}

class Post {
  final String id;
  final String userId;
  final String userName;
  final String? userAvatarUrl;
  final String content;
  final String? mediaUrl;
  final String? spotName;
  final int likesCount;
  final int commentsCount;
  bool isLikedByCurrentUser;
  final DateTime createdAtUtc;

  Post({
    required this.id,
    required this.userId,
    required this.userName,
    this.userAvatarUrl,
    required this.content,
    this.mediaUrl,
    this.spotName,
    required this.likesCount,
    required this.commentsCount,
    required this.isLikedByCurrentUser,
    required this.createdAtUtc,
  });

  factory Post.fromJson(Map<String, dynamic> json) {
    return Post(
      id: json['id'] ?? '',
      userId: json['userId'] ?? '',
      userName: json['userName'] ?? 'Kiter',
      userAvatarUrl: json['userAvatarUrl'],
      content: json['content'] ?? '',
      mediaUrl: json['mediaUrl'],
      spotName: json['spotName'],
      likesCount: json['likesCount'] ?? 0,
      commentsCount: json['commentsCount'] ?? 0,
      isLikedByCurrentUser: json['isLikedByCurrentUser'] ?? false,
      createdAtUtc: DateTime.tryParse(json['createdAtUtc'] ?? '') ?? DateTime.now(),
    );
  }
}
