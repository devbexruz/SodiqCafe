import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';

class AppRoutes {
  static final GoRouter router = GoRouter(
    initialLocation: '/',
    routes: [
      GoRoute(
        path: '/',
        builder: (context, state) => const Scaffold(
          body: Center(
            child: Text('Sodiq Cafe Admin - Bosh sahifa'),
          ),
        ),
      ),
    ],
  );
}
