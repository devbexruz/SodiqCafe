import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/features/menu/data/models/product_model.dart';
import 'package:sellermobile/features/menu/presentation/bloc/menu_bloc.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';
import 'package:sellermobile/features/menu/presentation/pages/add_from_template_page.dart';
import 'package:sellermobile/features/menu/presentation/pages/product_form_page.dart';

class AddProductBottomSheet extends StatefulWidget {
  const AddProductBottomSheet({super.key});

  @override
  State<AddProductBottomSheet> createState() => _AddProductBottomSheetState();
}

class _AddProductBottomSheetState extends State<AddProductBottomSheet>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  List<ProductModel> templates = [];
  bool isLoading = true;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    _loadTemplates();
  }

  Future<void> _loadTemplates() async {
    final menuBloc = context.read<MenuBloc>();
    final result = await menuBloc.menuRepository.getTemplates();
    result.fold(
      (failure) {
        if (mounted) {
          ScaffoldMessenger.of(context)
              .showSnackBar(SnackBar(content: Text(failure)));
          setState(() => isLoading = false);
        }
      },
      (data) {
        if (mounted) {
          setState(() {
            templates = data;
            isLoading = false;
          });
        }
      },
    );
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  IconData _categoryIcon(int categoryId) {
    switch (categoryId) {
      case 1: return Icons.local_cafe;
      case 2: return Icons.cake;
      case 3: return Icons.restaurant;
      default: return Icons.fastfood;
    }
  }

  Color _categoryColor(int categoryId) {
    switch (categoryId) {
      case 1: return const Color(0xFF2196F3);
      case 2: return const Color(0xFFE91E63);
      case 3: return const Color(0xFF4CAF50);
      default: return const Color(0xFF9E9E9E);
    }
  }

  void _openTemplate(ProductModel template) {
    final menuBloc = context.read<MenuBloc>();
    final dashState = context.read<DashboardCubit>().state;
    Navigator.pop(context); // close bottom sheet first
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => MultiBlocProvider(
          providers: [
            BlocProvider.value(value: menuBloc),
            if (dashState is DashboardLoaded)
              BlocProvider.value(value: context.read<DashboardCubit>()),
          ],
          child: AddFromTemplatePage(template: template),
        ),
      ),
    );
  }

  void _openCustomProductPage() {
    final menuBloc = context.read<MenuBloc>();
    final dashState = context.read<DashboardCubit>().state;
    if (dashState is! DashboardLoaded) return;
    final cafeId = dashState.cafes.id;
    final dashCubit = context.read<DashboardCubit>();
    Navigator.pop(context); // close bottom sheet
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => MultiBlocProvider(
          providers: [
            BlocProvider.value(value: menuBloc),
            BlocProvider.value(value: dashCubit),
          ],
          child: ProductFormPage(cafeId: cafeId),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final bottomPadding = MediaQuery.of(context).padding.bottom;

    return Container(
      height: MediaQuery.of(context).size.height * 0.82,
      decoration: const BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      child: Column(
        children: [
          // Drag handle
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 10),
            child: Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
          _buildHeader(),
          const Divider(height: 1),
          TabBar(
            controller: _tabController,
            labelColor: AppTheme.primary,
            unselectedLabelColor: Colors.black45,
            indicatorColor: AppTheme.primary,
            indicatorWeight: 3,
            labelStyle: const TextStyle(fontWeight: FontWeight.bold, fontSize: 13),
            tabs: const [
              Tab(text: '☕ Ichimlik'),
              Tab(text: '🍰 Shirinlik'),
              Tab(text: '🍽 Taom'),
            ],
          ),
          Expanded(
            child: isLoading
                ? const Center(child: CircularProgressIndicator())
                : TabBarView(
                    controller: _tabController,
                    children: [
                      _buildTemplateList(1),
                      _buildTemplateList(2),
                      _buildTemplateList(3),
                    ],
                  ),
          ),
          // Bottom button with proper safe area
          Container(
            color: Colors.white,
            padding: EdgeInsets.fromLTRB(16, 8, 16, 16 + bottomPadding),
            child: SizedBox(
              width: double.infinity,
              height: 52,
              child: ElevatedButton.icon(
                onPressed: _openCustomProductPage,
                icon: const Icon(Icons.add_circle_outline, size: 20),
                label: const Text(
                  'Boshqa mahsulot qo\'shish',
                  style: TextStyle(fontWeight: FontWeight.bold),
                ),
                style: ElevatedButton.styleFrom(
                  backgroundColor: AppTheme.primary,
                  foregroundColor: Colors.white,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(14),
                  ),
                  elevation: 0,
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildHeader() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          const Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Shablondan qo\'shish',
                style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              ),
              Text(
                'Kerakli mahsulotni tanlang',
                style: TextStyle(fontSize: 13, color: Colors.black45),
              ),
            ],
          ),
          IconButton(
            icon: const Icon(Icons.close),
            onPressed: () => Navigator.pop(context),
          ),
        ],
      ),
    );
  }

  Widget _buildTemplateList(int categoryId) {
    final categoryTemplates =
        templates.where((t) => t.category == categoryId).toList();

    if (categoryTemplates.isEmpty) {
      return Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.inbox_outlined, size: 48, color: Colors.grey.shade300),
            const SizedBox(height: 8),
            const Text(
              'Shablonlar topilmadi',
              style: TextStyle(color: Colors.black38),
            ),
          ],
        ),
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.fromLTRB(16, 12, 16, 8),
      itemCount: categoryTemplates.length,
      itemBuilder: (ctx, index) {
        final template = categoryTemplates[index];
        final catColor = _categoryColor(categoryId);
        return Card(
          margin: const EdgeInsets.only(bottom: 10),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          elevation: 0,
          color: Colors.grey.shade50,
          child: ListTile(
            contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
            leading: Container(
              width: 54,
              height: 54,
              decoration: BoxDecoration(
                color: catColor.withOpacity(0.12),
                borderRadius: BorderRadius.circular(12),
                image: template.imageUrl != null && template.imageUrl!.isNotEmpty
                    ? DecorationImage(
                        image: NetworkImage(template.imageUrl!),
                        fit: BoxFit.cover,
                      )
                    : null,
              ),
              child: (template.imageUrl == null || template.imageUrl!.isEmpty)
                  ? Icon(_categoryIcon(categoryId), color: catColor, size: 28)
                  : null,
            ),
            title: Text(
              template.name,
              style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 15),
            ),
            subtitle: Text(
              '${template.price.toStringAsFixed(0)} UZS',
              style: TextStyle(
                color: AppTheme.primary,
                fontWeight: FontWeight.w600,
                fontSize: 13,
              ),
            ),
            trailing: ElevatedButton(
              onPressed: () => _openTemplate(template),
              style: ElevatedButton.styleFrom(
                backgroundColor: AppTheme.primary,
                foregroundColor: Colors.white,
                elevation: 0,
                padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
              child: const Text(
                'Qo\'shish',
                style: TextStyle(fontSize: 13, fontWeight: FontWeight.bold),
              ),
            ),
          ),
        );
      },
    );
  }
}
