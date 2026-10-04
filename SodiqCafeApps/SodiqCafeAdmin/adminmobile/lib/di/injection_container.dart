import 'package:get_it/get_it.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:dio/dio.dart';

final sl = GetIt.instance;

Future<void> init() async {
  //! Core
  // Network
  sl.registerLazySingleton(() => Dio());

  //! External
  final sharedPreferences = await SharedPreferences.getInstance();
  sl.registerLazySingleton(() => sharedPreferences);
}
