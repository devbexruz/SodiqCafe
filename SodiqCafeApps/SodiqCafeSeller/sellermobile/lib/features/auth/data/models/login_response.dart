class LoginResponse {
  final bool success;
  final String? message;
  final String? sessionToken;
  final String? accessToken;
  final String? refreshToken;

  LoginResponse({
    required this.success,
    this.message,
    this.sessionToken,
    this.accessToken,
    this.refreshToken,
  });

  factory LoginResponse.fromJson(Map<String, dynamic> json) {
    // Handling standard API wrapper if exists
    final data = json['data'] ?? json;
    return LoginResponse(
      success: json['success'] ?? true, // Or standard structure
      message: json['message'] as String?,
      sessionToken: data['sessionToken'] as String?,
      accessToken: data['accessToken'] as String?,
      refreshToken: data['refreshToken'] as String?,
    );
  }
}
