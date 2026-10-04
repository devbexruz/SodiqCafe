import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:go_router/go_router.dart';
import '../bloc/dashboard_cubit.dart';
import 'package:sellermobile/features/main/presentation/bloc/main_cubit.dart';
import '../../../../di/injection_container.dart';
import '../../../../config/theme/app_theme.dart';
import '../../../../core/widgets/glass_container.dart';

class DashboardPage extends StatelessWidget {
  const DashboardPage({Key? key}) : super(key: key);

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      body: SafeArea(
        child: BlocBuilder<DashboardCubit, DashboardState>(
            builder: (context, state) {
              if (state is DashboardLoading) {
                return const Center(child: CircularProgressIndicator());
              } else if (state is DashboardLoaded) {
                return SingleChildScrollView(
                  padding: const EdgeInsets.all(16.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      // Header / Cafe Switcher
                      GlassContainer(
                        padding: const EdgeInsets.all(20),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            const Text(
                              'Xush kelibsiz!',
                              style: TextStyle(
                                fontSize: 18,
                                fontWeight: FontWeight.bold,
                                color: Colors.black87,
                              ),
                            ),
                            const SizedBox(height: 4),
                            const Text(
                              'Boshqarilayotgan kafe:',
                              style: TextStyle(color: Colors.black54, fontSize: 13),
                            ),
                            const SizedBox(height: 12),
                            Container(
                              padding: const EdgeInsets.symmetric(horizontal: 16),
                              decoration: BoxDecoration(
                                color: Colors.grey.withOpacity(0.1),
                                borderRadius: BorderRadius.circular(16),
                              ),
                              child: DropdownButtonHideUnderline(
                                child: DropdownButton<int>(
                                  isExpanded: true,
                                  value: state.cafes.id,
                                  icon: const Icon(Icons.keyboard_arrow_down, color: AppTheme.primary),
                                  dropdownColor: Colors.white,
                                  style: const TextStyle(
                                    color: AppTheme.primary,
                                    fontSize: 16,
                                    fontWeight: FontWeight.bold,
                                  ),
                                  onChanged: (int? newValue) {
                                    if (newValue != null) {
                                      context.read<DashboardCubit>().switchCafe(newValue);
                                    }
                                  },
                                  items: state.allCafes.map<DropdownMenuItem<int>>((dynamic value) {
                                    return DropdownMenuItem<int>(
                                      value: value['id'],
                                      child: Row(
                                        children: [
                                          const Icon(Icons.storefront, color: AppTheme.primary, size: 20),
                                          const SizedBox(width: 8),
                                          Text(value['name'], style: const TextStyle(color: Colors.black87)),
                                        ],
                                      ),
                                    );
                                  }).toList(),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      
                      const SizedBox(height: 20),

                      // Joriy Balans Card
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(24),
                        decoration: BoxDecoration(
                          gradient: const LinearGradient(
                            colors: [Color(0xFF0D6EFD), Color(0xFF6610F2)],
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight,
                          ),
                          borderRadius: BorderRadius.circular(24),
                          boxShadow: [
                            BoxShadow(
                              color: const Color(0xFF0D6EFD).withOpacity(0.3),
                              blurRadius: 20,
                              offset: const Offset(0, 10),
                            ),
                          ],
                        ),
                        child: Stack(
                          children: [
                            const Positioned(
                              right: -20,
                              bottom: -20,
                              child: Icon(Icons.account_balance_wallet, size: 100, color: Colors.white10),
                            ),
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Text(
                                  'JORIY BALANS',
                                  style: TextStyle(
                                    color: Colors.white70,
                                    fontSize: 12,
                                    fontWeight: FontWeight.bold,
                                    letterSpacing: 1.2,
                                  ),
                                ),
                                SizedBox(height: 8),
                                Row(
                                  crossAxisAlignment: CrossAxisAlignment.end,
                                  children: [
                                    Text(
                                      '${state.balance}',
                                      style: const TextStyle(
                                        color: Colors.white,
                                        fontSize: 36,
                                        fontWeight: FontWeight.w900,
                                      ),
                                    ),
                                    const SizedBox(width: 8),
                                    const Padding(
                                      padding: EdgeInsets.only(bottom: 6),
                                      child: Text(
                                        'UZS',
                                        style: TextStyle(
                                          color: Colors.white70,
                                          fontSize: 16,
                                          fontWeight: FontWeight.bold,
                                        ),
                                      ),
                                    ),
                                  ],
                                ),
                                const SizedBox(height: 8),
                                Row(
                                  children: [
                                    const Icon(Icons.info_outline, color: Colors.white70, size: 14),
                                    const SizedBox(width: 4),
                                    Text(
                                      'Oylik to\'lov: ${state.basePrice} UZS',
                                      style: const TextStyle(color: Colors.white70, fontSize: 12),
                                    ),
                                  ],
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                      
                      const SizedBox(height: 20),

                      // Grid Stats
                      GridView.count(
                        shrinkWrap: true,
                        physics: const NeverScrollableScrollPhysics(),
                        crossAxisCount: 2,
                        crossAxisSpacing: 16,
                        mainAxisSpacing: 16,
                        childAspectRatio: 1.1,
                        children: [
                          _buildStatCard(
                            icon: Icons.calendar_today,
                            value: '${state.daysLeft}',
                            label: 'kun qoldi',
                            gradientColors: const [Color(0xFF10B981), Color(0xFF059669)],
                          ),
                          _buildStatCard(
                            icon: Icons.coffee,
                            value: '${state.totalProducts}',
                            label: 'mahsulotlar',
                            gradientColors: const [Color(0xFF3B82F6), Color(0xFF2563EB)],
                            onTap: () => context.read<MainCubit>().changePage(1),
                          ),
                          _buildStatCard(
                            icon: Icons.people,
                            value: '${state.totalCustomers}',
                            label: 'mijozlar',
                            gradientColors: const [Color(0xFFF59E0B), Color(0xFFD97706)],
                            onTap: () => context.read<MainCubit>().changePage(3),
                          ),
                          _buildStatCard(
                            icon: Icons.card_giftcard,
                            value: '${state.totalBonusesGiven}',
                            label: 'berilgan bonuslar',
                            gradientColors: const [Color(0xFFEF4444), Color(0xFFDC2626)],
                            onTap: () => context.read<MainCubit>().changePage(4),
                          ),
                        ],
                      ),
                    ],
                  ),
                );
              } else if (state is DashboardError) {
                return Center(child: Text(state.message, style: const TextStyle(color: Colors.red, fontWeight: FontWeight.bold)));
              }
              return const SizedBox();
            },
          ),
        ),
    );
  }

  Widget _buildStatCard({
    required IconData icon,
    required String value,
    required String label,
    required List<Color> gradientColors,
    VoidCallback? onTap,
  }) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(20),
      child: Container(
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: Colors.black.withOpacity(0.03),
              blurRadius: 15,
              offset: const Offset(0, 5),
            ),
          ],
        ),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
          Container(
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              gradient: LinearGradient(
                colors: gradientColors,
                begin: Alignment.topLeft,
                end: Alignment.bottomRight,
              ),
              borderRadius: BorderRadius.circular(16),
            ),
            child: Icon(icon, color: Colors.white, size: 24),
          ),
          const SizedBox(height: 12),
          Text(
            value,
            style: const TextStyle(fontSize: 22, fontWeight: FontWeight.bold, color: Colors.black87),
          ),
          const SizedBox(height: 2),
          Text(
            label,
            style: const TextStyle(fontSize: 12, color: Colors.black54),
          ),
        ],
      ),
    ),
    );
  }
}
