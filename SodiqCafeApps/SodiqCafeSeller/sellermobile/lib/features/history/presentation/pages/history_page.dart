import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/core/widgets/glass_container.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';
import 'package:sellermobile/features/history/presentation/bloc/history_cubit.dart';
import 'package:sellermobile/di/injection_container.dart';
import 'package:intl/intl.dart';

class HistoryPage extends StatelessWidget {
  const HistoryPage({super.key});

  @override
  Widget build(BuildContext context) {
    return BlocProvider(
      create: (context) {
        final cafeId = (context.read<DashboardCubit>().state as DashboardLoaded).cafes.id;
        return sl<HistoryCubit>()..loadHistory(cafeId);
      },
      child: const HistoryView(),
    );
  }
}

class HistoryView extends StatelessWidget {
  const HistoryView({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: GlassContainer(
                padding: const EdgeInsets.all(20),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Text(
                          '🕒 Sotuvlar Tarixi',
                          style: TextStyle(
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                            color: Colors.black87,
                          ),
                        ),
                        SizedBox(height: 4),
                        Text(
                          'Bugungi yakunlangan xizmatlar',
                          style: TextStyle(color: Colors.black54, fontSize: 13),
                        ),
                      ],
                    ),
                    IconButton(
                      icon: const Icon(Icons.refresh, color: AppTheme.primary),
                      onPressed: () {
                        final cafeId = (context.read<DashboardCubit>().state as DashboardLoaded).cafes.id;
                        context.read<HistoryCubit>().loadHistory(cafeId);
                      },
                    ),
                  ],
                ),
              ),
            ),
            Expanded(
              child: BlocBuilder<HistoryCubit, HistoryState>(
                builder: (context, state) {
                  if (state is HistoryLoading) {
                    return const Center(child: CircularProgressIndicator());
                  } else if (state is HistoryError) {
                    return Center(child: Text(state.message, style: const TextStyle(color: Colors.red)));
                  } else if (state is HistoryLoaded) {
                    if (state.history.isEmpty) {
                      return const Center(
                        child: Text('Bugun hali yakunlangan buyurtmalar yo\'q', style: TextStyle(color: Colors.black54)),
                      );
                    }
                    return ListView.builder(
                      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
                      itemCount: state.history.length,
                      itemBuilder: (context, index) {
                        return _buildHistoryCard(state.history[index], state.rewards);
                      },
                    );
                  }
                  return const SizedBox.shrink();
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHistoryCard(dynamic req, List<dynamic> allRewards) {
    // Determine if this request got a bonus
    // We match by userId and completion time within a 2-minute margin
    final completedAt = req['completedAt'] != null ? DateTime.parse(req['completedAt']).toLocal() : null;
    final userId = req['userId'];
    
    List<dynamic> matchedRewards = [];
    if (userId != null && completedAt != null) {
      matchedRewards = allRewards.where((r) {
        if (r['userId'] != userId) return false;
        final earnedAt = DateTime.parse(r['earnedAt']).toLocal();
        return earnedAt.difference(completedAt).inMinutes.abs() < 2;
      }).toList();
    }

    final bool hasBonus = matchedRewards.isNotEmpty;

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      decoration: BoxDecoration(
        color: hasBonus ? Colors.green.withOpacity(0.05) : Colors.white,
        borderRadius: BorderRadius.circular(16),
        border: hasBonus ? Border.all(color: Colors.green.withOpacity(0.3)) : null,
        boxShadow: hasBonus ? [] : [
          BoxShadow(
            color: Colors.black.withOpacity(0.03),
            blurRadius: 10,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Text(
                '#${req['dailyOrderNumber']}',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
              ),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                decoration: BoxDecoration(
                  color: Colors.grey.withOpacity(0.2),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: const Text('Yakunlangan', style: TextStyle(fontSize: 12, fontWeight: FontWeight.bold, color: Colors.black54)),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            children: [
              CircleAvatar(
                backgroundColor: req['user'] != null ? AppTheme.primary : Colors.grey,
                radius: 20,
                child: Text(
                  req['user'] != null ? (req['user']['fullName'] as String).substring(0, 1).toUpperCase() : 'G',
                  style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      req['user'] != null ? req['user']['fullName'] : 'Mehmon (Ro\'yxatdan o\'tmagan)',
                      style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
                    ),
                    if (req['user'] != null && req['user']['phoneNumber'] != null)
                      Text(req['user']['phoneNumber'], style: const TextStyle(color: Colors.black54, fontSize: 12)),
                  ],
                ),
              ),
              Text(
                completedAt != null ? DateFormat('HH:mm').format(completedAt) : '-',
                style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
              ),
            ],
          ),
          if (hasBonus) ...[
            const SizedBox(height: 12),
            Wrap(
              spacing: 8,
              runSpacing: 4,
              children: matchedRewards.map((r) {
                final bonusName = r['bonusCampaign']['name'];
                return Container(
                  padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.green.withOpacity(0.15),
                    borderRadius: BorderRadius.circular(8),
                  ),
                  child: Row(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      const Icon(Icons.card_giftcard, size: 14, color: Colors.green),
                      const SizedBox(width: 4),
                      Text(bonusName, style: const TextStyle(color: Colors.green, fontSize: 12, fontWeight: FontWeight.bold)),
                    ],
                  ),
                );
              }).toList(),
            ),
          ],
        ],
      ),
    );
  }
}
