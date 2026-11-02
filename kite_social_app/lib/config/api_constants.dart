import 'dart:io' show Platform;
import 'package:flutter/foundation.dart' show kIsWeb;

class ApiConstants {
  // Configured dynamically depending on whether running in Web, Android Emulator, iOS Simulator or Desktop
  static String get baseUrl {
    if (kIsWeb) {
      return 'http://localhost:5067';
    }
    if (Platform.isAndroid) {
      // Android emulator maps 10.0.2.2 to host machine's localhost
      return 'http://10.0.2.2:5067';
    }
    // iOS Simulator, macOS, Windows, Linux
    return 'http://localhost:5067';
  }

  // Endpoints
  static const String login = '/api/auth/login';
  static const String register = '/api/auth/register';
  static const String currentUser = '/api/auth/me';
  static const String spots = '/api/spots';
  static const String sessions = '/api/sessions';
  static const String mySessions = '/api/sessions/my';
  static const String sessionStats = '/api/sessions/stats';
  static const String posts = '/api/posts';
  static const String health = '/api/health';
}
