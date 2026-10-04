import 'package:flutter/material.dart';

class AppTheme {
  static const Color primary = Color(0xFF0D6EFD);
  static const Color primaryDark = Color(0xFF6610F2);
  
  static const Color success = Color(0xFF10B981);
  static const Color successDark = Color(0xFF059669);
  
  static const Color info = Color(0xFF3B82F6);
  static const Color infoDark = Color(0xFF2563EB);
  
  static const Color warning = Color(0xFFF59E0B);
  static const Color warningDark = Color(0xFFD97706);
  
  static const Color danger = Color(0xFFEF4444);
  static const Color dangerDark = Color(0xFFDC2626);

  static const Color background = Color(0xFFF8F9FA);
  
  // Gradients
  static const LinearGradient balanceGradient = LinearGradient(
    colors: [primary, primaryDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
  
  static const LinearGradient activeGradient = LinearGradient(
    colors: [success, successDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
  
  static const LinearGradient productsGradient = LinearGradient(
    colors: [info, infoDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
  
  static const LinearGradient customersGradient = LinearGradient(
    colors: [warning, warningDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );
  
  static const LinearGradient bonusesGradient = LinearGradient(
    colors: [danger, dangerDark],
    begin: Alignment.topLeft,
    end: Alignment.bottomRight,
  );

  static ThemeData get lightTheme {
    return ThemeData(
      useMaterial3: true,
      colorScheme: ColorScheme.fromSeed(
        seedColor: primary,
        background: background,
      ),
      scaffoldBackgroundColor: background,
      appBarTheme: const AppBarTheme(
        backgroundColor: Colors.transparent,
        elevation: 0,
        centerTitle: false,
        iconTheme: IconThemeData(color: Colors.black87),
        titleTextStyle: TextStyle(
          color: Colors.black87,
          fontSize: 20,
          fontWeight: FontWeight.bold,
        ),
      ),
      cardTheme: CardThemeData(
        elevation: 0,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(20),
        ),
      ),
    );
  }
}
