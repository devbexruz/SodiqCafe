import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../bloc/menu_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/core/widgets/glass_container.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';
import 'package:sellermobile/features/menu/presentation/pages/bonus_management_page.dart';
import 'package:sellermobile/features/menu/presentation/widgets/add_product_bottom_sheet.dart';
import 'package:sellermobile/features/menu/data/models/product_model.dart';
import 'package:sellermobile/features/menu/presentation/pages/product_form_page.dart';

class ProductsPage extends StatefulWidget {
  const ProductsPage({super.key});

  @override
  State<ProductsPage> createState() => _ProductsPageState();
}

class _ProductsPageState extends State<ProductsPage> {
  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Column(
          children: [
            Padding(
              padding: const EdgeInsets.all(16.0),
              child: _buildHeader(context),
            ),
            Expanded(
              child: BlocBuilder<MenuBloc, MenuState>(
                builder: (context, state) {
                  if (state is MenuLoading) {
                    return const Center(child: CircularProgressIndicator());
                  } else if (state is MenuError) {
                    return Center(
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(state.message, style: const TextStyle(color: Colors.red)),
                          const SizedBox(height: 16),
                          ElevatedButton(
                            onPressed: () {
                              final dashboardState = context.read<DashboardCubit>().state;
                              if (dashboardState is DashboardLoaded) {
                                context.read<MenuBloc>().add(LoadMenuEvent(cafeId: dashboardState.cafes.id));
                              }
                            },
                            child: const Text('Qayta urinib ko\'rish'),
                          ),
                        ],
                      ),
                    );
                  } else if (state is MenuLoaded) {
                    return ListView(
                      padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 8.0),
                      children: [
                        if (state.activeBonuses.isNotEmpty)
                          _buildActiveBonusesSlider(state.activeBonuses),
                        // Section title row
                        Padding(
                          padding: const EdgeInsets.fromLTRB(0, 12, 0, 8),
                          child: Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: [
                              const Text(
                                'Maxsulotlar',
                                style: TextStyle(
                                  fontSize: 18,
                                  fontWeight: FontWeight.bold,
                                  color: Colors.black87,
                                ),
                              ),
                              TextButton.icon(
                                onPressed: () => _showAddProductBottomSheet(context),
                                icon: const Icon(Icons.add_circle, size: 18),
                                label: const Text(
                                  'Yangi',
                                  style: TextStyle(fontWeight: FontWeight.bold),
                                ),
                                style: TextButton.styleFrom(
                                  foregroundColor: AppTheme.primary,
                                  backgroundColor: AppTheme.primary.withOpacity(0.08),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(10),
                                  ),
                                  padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                                ),
                              ),
                            ],
                          ),
                        ),
                        if (state.products.isEmpty)
                          _buildEmptyState()
                        else
                          ...state.products.map((p) => _buildProductCard(p)),
                      ],
                    );
                  }
                  return const Center(child: Text('Ma\'lumotlar yuklanmoqda...'));
                },
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader(BuildContext context) {
    return GlassContainer(
      padding: const EdgeInsets.all(20),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          const Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                '🍔 Menyuni Boshqarish',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: Colors.black87,
                ),
              ),
              SizedBox(height: 4),
              Text(
                'Kafe mahsulotlarini sozlash',
                style: TextStyle(color: Colors.black54, fontSize: 13),
              ),
            ],
          ),
          ElevatedButton.icon(
            onPressed: () {
              final dashboardCubit = context.read<DashboardCubit>();
              Navigator.push(context, MaterialPageRoute(builder: (_) => BlocProvider.value(
                value: dashboardCubit,
                child: const BonusManagementPage(),
              )));
            },
            icon: const Icon(Icons.settings, size: 16, color: AppTheme.primary),
            label: const Text('Boshqarish', style: TextStyle(color: AppTheme.primary, fontWeight: FontWeight.bold)),
            style: ElevatedButton.styleFrom(
              backgroundColor: Colors.white,
              elevation: 2,
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
            ),
          ),
        ],
      ),
    );
  }

  String _categoryName(int? category) {
    switch (category) {
      case 1: return 'Ichimlik';
      case 2: return 'Shirinlik';
      case 3: return 'Taom';
      default: return 'Boshqa';
    }
  }

  IconData _categoryIcon(int? category) {
    switch (category) {
      case 1: return Icons.local_cafe;       // Ichimliklar — kofe finjoni
      case 2: return Icons.cake;             // Shirinliklar — tort
      case 3: return Icons.restaurant;       // Taomlar — vilka-qoshiq
      default: return Icons.fastfood;
    }
  }

  Color _categoryColor(int? category) {
    switch (category) {
      case 1: return const Color(0xFF2196F3); // Ko'k — ichimlik
      case 2: return const Color(0xFFE91E63); // Pushti — shirinlik
      case 3: return const Color(0xFF4CAF50); // Yashil — taom
      default: return const Color(0xFF9E9E9E);
    }
  }

  Widget _buildEmptyState() {
    return const Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Text('🍽️', style: TextStyle(fontSize: 48)),
          SizedBox(height: 16),
          Text(
            'Hozircha menyuda mahsulotlar yo\'q',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.black54),
          ),
          SizedBox(height: 8),
          Text(
            'Qo\'shish tugmasini bosib yangi mahsulot kiritishingiz mumkin.',
            style: TextStyle(color: Colors.black45, fontSize: 13),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }

  Widget _buildProductCard(dynamic product) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
      child: Padding(
        padding: const EdgeInsets.all(12.0),
        child: Row(
          children: [
            Container(
              width: 80,
              height: 80,
              decoration: BoxDecoration(
                color: _categoryColor(product.category).withOpacity(0.12),
                borderRadius: BorderRadius.circular(16),
                image: product.imageUrl != null && product.imageUrl.isNotEmpty
                    ? DecorationImage(
                        image: NetworkImage(product.imageUrl),
                        fit: BoxFit.cover,
                      )
                    : null,
              ),
              child: (product.imageUrl == null || product.imageUrl.isEmpty)
                  ? Icon(_categoryIcon(product.category), size: 36, color: _categoryColor(product.category))
                  : null,
            ),
            const SizedBox(width: 16),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                    decoration: BoxDecoration(
                      color: _categoryColor(product.category).withOpacity(0.1),
                      borderRadius: BorderRadius.circular(8),
                    ),
                    child: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        Icon(_categoryIcon(product.category), size: 11, color: _categoryColor(product.category)),
                        const SizedBox(width: 4),
                        Text(
                          _categoryName(product.category),
                          style: TextStyle(fontSize: 10, fontWeight: FontWeight.bold, color: _categoryColor(product.category)),
                        ),
                      ],
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    product.name,
                    style: const TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.bold,
                      color: Colors.black87,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '${product.price.toStringAsFixed(0)} UZS',
                    style: const TextStyle(
                      fontSize: 15,
                      fontWeight: FontWeight.bold,
                      color: AppTheme.success,
                    ),
                  ),
                ],
              ),
            ),
            Column(
              children: [
                IconButton(
                  onPressed: () => _showEditProductDialog(context, product),
                  icon: const Icon(Icons.edit, color: AppTheme.primary),
                  style: IconButton.styleFrom(
                    backgroundColor: AppTheme.primary.withOpacity(0.1),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                ),
                const SizedBox(height: 4),
                IconButton(
                  onPressed: () => _confirmDeleteProduct(context, product),
                  icon: const Icon(Icons.delete, color: AppTheme.danger),
                  style: IconButton.styleFrom(
                    backgroundColor: AppTheme.danger.withOpacity(0.1),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  void _showEditProductDialog(BuildContext context, ProductModel product) {
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
          child: ProductFormPage(product: product, cafeId: cafeId),
        ),
      ),
    );
  }

  void _confirmDeleteProduct(BuildContext context, ProductModel product) {
    final menuBloc = context.read<MenuBloc>();
    final dashState = context.read<DashboardCubit>().state;
    if (dashState is! DashboardLoaded) return;
    final cafeId = dashState.cafes.id;
    final messenger = ScaffoldMessenger.of(context);

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(20)),
        icon: const Icon(Icons.delete_outline, color: AppTheme.danger, size: 36),
        title: const Text(
          'O\'chirishni tasdiqlang',
          textAlign: TextAlign.center,
          style: TextStyle(fontWeight: FontWeight.bold),
        ),
        content: Text(
          '"${product.name}" mahsuloti ro\'yxatdan butunlay o\'chiriladi.',
          textAlign: TextAlign.center,
          style: const TextStyle(color: Colors.black54),
        ),
        actions: [
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: () => Navigator.pop(ctx),
                  style: OutlinedButton.styleFrom(
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    padding: const EdgeInsets.symmetric(vertical: 14),
                  ),
                  child: const Text('Bekor qilish'),
                ),
              ),
              const SizedBox(width: 8),
              Expanded(
                child: ElevatedButton(
                  onPressed: () {
                    final nav = Navigator.of(ctx);
                    menuBloc.add(DeleteProductEvent(productId: product.id, cafeId: cafeId));
                    nav.pop();
                    messenger.showSnackBar(
                      SnackBar(
                        content: Text('${product.name} o\'chirildi'),
                        backgroundColor: AppTheme.danger,
                        behavior: SnackBarBehavior.floating,
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                      ),
                    );
                  },
                  style: ElevatedButton.styleFrom(
                    backgroundColor: AppTheme.danger,
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    elevation: 0,
                  ),
                  child: const Text('O\'chirish'),
                ),
              ),
            ],
          ),
          const SizedBox(height: 4),
        ],
      ),
    );
  }

  Widget _buildActiveBonusesSlider(List<dynamic> activeBonuses) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Padding(
          padding: EdgeInsets.only(bottom: 8.0),
          child: Text(
            'Faol Bonuslar',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.bold,
              color: Colors.black87,
            ),
          ),
        ),
        SizedBox(
          height: 140,
          child: ListView.builder(
            scrollDirection: Axis.horizontal,
            itemCount: activeBonuses.length,
            itemBuilder: (context, index) {
              final bonus = activeBonuses[index];
              return Container(
                width: 280,
                margin: const EdgeInsets.only(right: 12, bottom: 8),
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [AppTheme.primary, AppTheme.primaryDark],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(16),
                  boxShadow: [
                    BoxShadow(
                      color: AppTheme.primary.withOpacity(0.3),
                      blurRadius: 8,
                      offset: const Offset(0, 4),
                    ),
                  ],
                ),
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        const Icon(Icons.star, color: Colors.amber, size: 20),
                        const SizedBox(width: 8),
                        Expanded(
                          child: Text(
                            bonus.name,
                            style: const TextStyle(
                              color: Colors.white,
                              fontWeight: FontWeight.bold,
                              fontSize: 16,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                      ],
                    ),
                    const SizedBox(height: 12),
                    Text(
                      'Shart: ${bonus.conditionValue.toStringAsFixed(0)}',
                      style: const TextStyle(color: Colors.white70, fontSize: 13),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Mukofot: ${bonus.rewardDescription}',
                      style: const TextStyle(color: Colors.white, fontSize: 13, fontWeight: FontWeight.w500),
                      maxLines: 2,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ],
                ),
              );
            },
          ),
        ),
        const SizedBox(height: 8),
      ],
    );
  }

  void _showAddProductBottomSheet(BuildContext context) {
    showModalBottomSheet(
      context: context,
      isScrollControlled: true,
      backgroundColor: Colors.transparent,
      builder: (_) => BlocProvider.value(
        value: context.read<MenuBloc>(),
        child: BlocProvider.value(
          value: context.read<DashboardCubit>(),
          child: const AddProductBottomSheet(),
        ),
      ),
    );
  }
}

