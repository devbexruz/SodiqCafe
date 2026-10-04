import 'package:dartz/dartz.dart';
import '../../data/models/login_response.dart';

abstract class AuthRepository {
  Future<Either<String, LoginResponse>> login(String usernameOrEmail, String password);
  Future<void> logout();
  Future<bool> isLoggedIn();
  Future<String?> getToken();
}
