import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/features/menu/presentation/bloc/menu_bloc.dart';
import 'package:sellermobile/features/menu/data/models/bonus_campaign_model.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';
import 'package:sellermobile/features/menu/presentation/pages/add_bonus_page.dart';

class BonusManagementPage extends StatelessWidget {
  const BonusManagementPage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppTheme.background,
      appBar: AppBar(
        title: const Text('Bonuslarni Boshqarish', style: TextStyle(fontWeight: FontWeight.bold)),
        backgroundColor: Colors.white,
        elevation: 0,
        centerTitle: true,
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () {
          final dashState = context.read<DashboardCubit>().state;
          if (dashState is! DashboardLoaded) return;
          final cafeId = dashState.cafes.id;
          final menuBloc = context.read<MenuBloc>();
          final dashCubit = context.read<DashboardCubit>();

          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (_) => MultiBlocProvider(
                providers: [
                  BlocProvider.value(value: menuBloc),
                  BlocProvider.value(value: dashCubit),
                ],
                child: AddBonusPage(cafeId: cafeId),
              ),
            ),
          );
        },
        backgroundColor: AppTheme.primary,
        icon: const Icon(Icons.add, color: Colors.white),
        label: const Text('Yangi Aksiya', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
      ),
      body: BlocBuilder<MenuBloc, MenuState>(
        builder: (context, state) {
          if (state is MenuLoading) {
            return const Center(child: CircularProgressIndicator());
          } else if (state is MenuLoaded) {
            final activeBonuses = state.allBonuses.where((b) => b.isActive).toList();
            final pausedBonuses = state.allBonuses.where((b) => !b.isActive).toList();
            
            if (state.allBonuses.isEmpty) {
              return const Center(
                child: Text('Hozircha aksiyalar mavjud emas.', style: TextStyle(color: Colors.black54)),
              );
            }
            
            return CustomScrollView(
              slivers: [
                if (activeBonuses.isNotEmpty) ...[
                  _buildSectionHeader('Faol Aksiyalar', Colors.green),
                  _buildBonusList(activeBonuses, true, context),
                ],
                if (pausedBonuses.isNotEmpty) ...[
                  _buildSectionHeader('To\'xtatilgan Aksiyalar', Colors.red),
                  _buildBonusList(pausedBonuses, false, context),
                ],
                const SliverToBoxAdapter(child: SizedBox(height: 80)), // padding for FAB
              ],
            );
          }
          return const Center(child: Text('Xatolik yuz berdi.'));
        },
      ),
    );
  }

  Widget _buildSectionHeader(String title, Color color) {
    return SliverToBoxAdapter(
      child: Padding(
        padding: const EdgeInsets.fromLTRB(16, 24, 16, 8),
        child: Row(
          children: [
            Container(
              width: 4,
              height: 20,
              decoration: BoxDecoration(
                color: color,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
            const SizedBox(width: 8),
            Text(
              title,
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
                color: Colors.black87,
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBonusList(List<BonusCampaignModel> bonuses, bool isActive, BuildContext context) {
    final cafeId = (context.read<DashboardCubit>().state as DashboardLoaded).cafes.id;
    return SliverList(
      delegate: SliverChildBuilderDelegate(
        (context, index) {
          final bonus = bonuses[index];
          return Container(
            margin: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
            decoration: BoxDecoration(
              color: Colors.white,
              borderRadius: BorderRadius.circular(16),
              boxShadow: [
                BoxShadow(
                  color: Colors.black.withOpacity(0.05),
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
                    Expanded(
                      child: Text(
                        bonus.name,
                        style: const TextStyle(
                          fontSize: 18,
                          fontWeight: FontWeight.bold,
                          color: AppTheme.primary,
                        ),
                      ),
                    ),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                      decoration: BoxDecoration(
                        color: isActive ? Colors.green.withOpacity(0.1) : Colors.red.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(20),
                      ),
                      child: Text(
                        isActive ? 'Faol' : 'To\'xtatilgan',
                        style: TextStyle(
                          color: isActive ? Colors.green : Colors.red,
                          fontWeight: FontWeight.bold,
                          fontSize: 12,
                        ),
                      ),
                    ),
                  ],
                ),
                if (bonus.description != null && bonus.description!.isNotEmpty) ...[
                  const SizedBox(height: 8),
                  Text(
                    bonus.description!,
                    style: const TextStyle(color: Colors.black54, fontSize: 13),
                  ),
                ],
                const SizedBox(height: 12),
                Row(
                  children: [
                    _buildBadge(Icons.flag, 'Shart: ${bonus.conditionValue.toStringAsFixed(0)}', Colors.blue),
                    const SizedBox(width: 8),
                    _buildBadge(
                      bonus.isVisible ? Icons.visibility : Icons.visibility_off, 
                      bonus.isVisible ? 'Ochiq' : 'Yashirin', 
                      bonus.isVisible ? Colors.green : Colors.grey
                    ),
                  ],
                ),
                const SizedBox(height: 12),
                Container(
                  width: double.infinity,
                  padding: const EdgeInsets.all(12),
                  decoration: BoxDecoration(
                    color: AppTheme.primary.withOpacity(0.05),
                    borderRadius: BorderRadius.circular(8),
                    border: Border.all(color: AppTheme.primary.withOpacity(0.2)),
                  ),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Row(
                        children: [
                          Icon(Icons.card_giftcard, size: 16, color: AppTheme.primary),
                          SizedBox(width: 6),
                          Text('Mukofot', style: TextStyle(color: AppTheme.primary, fontWeight: FontWeight.bold)),
                        ],
                      ),
                      const SizedBox(height: 4),
                      Text(bonus.rewardDescription, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 14)),
                    ],
                  ),
                ),
                const SizedBox(height: 16),
                Row(
                  mainAxisAlignment: MainAxisAlignment.end,
                  children: [
                    ElevatedButton.icon(
                      onPressed: () {
                        context.read<MenuBloc>().add(ToggleBonusEvent(campaignId: bonus.id, cafeId: cafeId));
                      },
                      style: ElevatedButton.styleFrom(
                        backgroundColor: isActive ? Colors.orange : Colors.green,
                        foregroundColor: Colors.white,
                        elevation: 0,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                      ),
                      icon: Icon(isActive ? Icons.pause : Icons.play_arrow, size: 18),
                      label: Text(isActive ? 'To\'xtatish' : 'Faollashtirish'),
                    ),
                    const SizedBox(width: 8),
                    IconButton(
                      onPressed: () {
                        showDialog(
                          context: context,
                          builder: (c) => AlertDialog(
                            title: const Text('O\'chirish'),
                            content: const Text('Ushbu e\'lonni haqiqatan ham o\'chirasizmi?'),
                            actions: [
                              TextButton(onPressed: () => Navigator.pop(c), child: const Text('Bekor qilish')),
                              TextButton(
                                onPressed: () {
                                  context.read<MenuBloc>().add(DeleteBonusEvent(campaignId: bonus.id, cafeId: cafeId));
                                  Navigator.pop(c);
                                },
                                child: const Text('O\'chirish', style: TextStyle(color: Colors.red)),
                              ),
                            ],
                          ),
                        );
                      },
                      icon: const Icon(Icons.delete_outline, color: Colors.red),
                      style: IconButton.styleFrom(
                        backgroundColor: Colors.red.withOpacity(0.1),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                      ),
                    ),
                  ],
                ),
              ],
            ),
          );
        },
        childCount: bonuses.length,
      ),
    );
  }

  Widget _buildBadge(IconData icon, String text, Color color) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: color.withOpacity(0.1),
        borderRadius: BorderRadius.circular(6),
        border: Border.all(color: color.withOpacity(0.3)),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 14, color: color),
          const SizedBox(width: 4),
          Text(text, style: TextStyle(color: color, fontSize: 12, fontWeight: FontWeight.bold)),
        ],
      ),
    );
  }

  void _showCreateBonusDialog(BuildContext context) {
    final nameController = TextEditingController();
    final descController = TextEditingController();
    final conditionController = TextEditingController();
    final rewardController = TextEditingController();
    bool isVisible = true;

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (innerCtx, setStateSB) => AlertDialog(
          title: const Text('Yangi Aksiya Yaratish'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: nameController,
                  decoration: const InputDecoration(labelText: 'Aksiya Nomi', border: OutlineInputBorder()),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: descController,
                  maxLines: 2,
                  decoration: const InputDecoration(labelText: 'Tavsif (ixtiyoriy)', border: OutlineInputBorder()),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: conditionController,
                  keyboardType: TextInputType.number,
                  decoration: const InputDecoration(
                    labelText: 'Shart miqdori',
                    border: OutlineInputBorder(),
                    hintText: 'Masalan: 5 (5ta stakan kofe uchun)',
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: rewardController,
                  maxLines: 2,
                  decoration: const InputDecoration(
                    labelText: 'Mukofot nima?',
                    border: OutlineInputBorder(),
                    hintText: 'Masalan: 1 stakan bepul Americano',
                  ),
                ),
                const SizedBox(height: 12),
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text('Mijozlarga ko\'rinadimi?'),
                    Switch(
                      value: isVisible,
                      onChanged: (val) => setStateSB(() => isVisible = val),
                    ),
                  ],
                ),
              ],
            ),
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(ctx), child: const Text('Bekor qilish')),
            ElevatedButton(
              onPressed: () {
                final condition = double.tryParse(conditionController.text) ?? 0;
                if (nameController.text.isEmpty || condition <= 0 || rewardController.text.isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(const SnackBar(content: Text('Barcha maydonlarni to\'g\'ri to\'ldiring')));
                  return;
                }

                final cafeId = (context.read<DashboardCubit>().state as DashboardLoaded).cafes.id;

                final bonus = BonusCampaignModel(
                  id: 0,
                  cafeId: cafeId,
                  name: nameController.text,
                  description: descController.text,
                  type: 0,
                  conditionValue: condition,
                  rewardDescription: rewardController.text,
                  isActive: true,
                  isVisible: isVisible,
                );

                final menuBloc = context.read<MenuBloc>();
                final messenger = ScaffoldMessenger.of(context);
                final nav = Navigator.of(ctx);
                
                menuBloc.add(AddBonusCampaignEvent(campaign: bonus, cafeId: cafeId));
                
                nav.pop();
                messenger.showSnackBar(const SnackBar(content: Text('Aksiya yaratilmoqda...')));
              },
              child: const Text('Saqlash'),
            ),
          ],
        ),
      ),
    );
  }
}
