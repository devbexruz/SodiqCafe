import 'package:flutter/material.dart';
import 'package:flutter_bloc/flutter_bloc.dart';
import '../bloc/main_cubit.dart';
import 'package:sellermobile/config/theme/app_theme.dart';
import 'package:sellermobile/features/dashboard/presentation/pages/dashboard_page.dart';
import 'package:sellermobile/features/menu/presentation/pages/products_page.dart';
import 'package:sellermobile/features/queue/presentation/pages/queue_page.dart';
import 'package:sellermobile/features/customers/presentation/pages/customers_page.dart';
import 'package:sellermobile/features/history/presentation/pages/history_page.dart';
import 'package:sellermobile/features/invoices/presentation/pages/invoices_page.dart';
import 'package:sellermobile/features/settings/presentation/pages/settings_page.dart';
import 'package:sellermobile/features/dashboard/presentation/bloc/dashboard_cubit.dart';
import 'package:sellermobile/features/menu/presentation/bloc/menu_bloc.dart';
import 'package:sellermobile/di/injection_container.dart';

class MainPage extends StatelessWidget {
  const MainPage({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiBlocProvider(
      providers: [
        BlocProvider(create: (context) => MainCubit()),
        BlocProvider(create: (context) => sl<DashboardCubit>()..loadDashboard()),
      ],
      child: BlocListener<DashboardCubit, DashboardState>(
        listener: (context, state) {
          if (state is DashboardLoaded) {
            context.read<MenuBloc>().add(LoadMenuEvent(cafeId: state.cafes.id));
          }
        },
        child: const MainView(),
      ),
    );
  }
}

class MainView extends StatelessWidget {
  const MainView({super.key});

  final List<Widget> _pages = const [
    DashboardPage(),
    ProductsPage(),
    QueuePage(),
    CustomersPage(),
    HistoryPage(),
    InvoicesPage(),
    SettingsPage(),
  ];

  @override
  Widget build(BuildContext context) {
    final isDesktop = MediaQuery.of(context).size.width >= 800;
    final scaffoldKey = GlobalKey<ScaffoldState>();

    return BlocBuilder<MainCubit, int>(
      builder: (context, currentIndex) {
        return Scaffold(
          key: scaffoldKey,
          extendBodyBehindAppBar: true,
          appBar: null,
          endDrawer: isDesktop
              ? null
              : Drawer(
                  child: ListView(
                    padding: EdgeInsets.zero,
                    children: [
                      const DrawerHeader(
                        decoration: BoxDecoration(
                          gradient: LinearGradient(
                            colors: [Color(0xFF0D6EFD), Color(0xFF6610F2)],
                          ),
                        ),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          mainAxisAlignment: MainAxisAlignment.end,
                          children: [
                            CircleAvatar(
                              backgroundColor: Colors.white,
                              radius: 30,
                              child: Icon(Icons.store, size: 35, color: Color(0xFF0D6EFD)),
                            ),
                            SizedBox(height: 12),
                            Text(
                              'Sotuvchi Menyu',
                              style: TextStyle(color: Colors.white, fontSize: 20, fontWeight: FontWeight.bold),
                            ),
                          ],
                        ),
                      ),
                      _buildDrawerItem(context, 3, Icons.people_alt, Colors.amber, 'Mijozlar', currentIndex),
                      _buildDrawerItem(context, 4, Icons.history, Colors.blue, 'Tarix', currentIndex),
                      _buildDrawerItem(context, 5, Icons.receipt, Colors.indigo, 'Invoyslar', currentIndex),
                      _buildDrawerItem(context, 6, Icons.settings, Colors.grey, 'Sozlamalar', currentIndex),
                    ],
                  ),
                ),
          body: LayoutBuilder(
            builder: (context, constraints) {
              if (constraints.maxWidth >= 800) {
                return Row(
                  children: [
                    NavigationRail(
                      selectedIndex: currentIndex,
                      onDestinationSelected: (index) => context.read<MainCubit>().changePage(index),
                      labelType: NavigationRailLabelType.all,
                      selectedLabelTextStyle: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF0D6EFD)),
                      selectedIconTheme: const IconThemeData(color: Color(0xFF0D6EFD)),
                      destinations: const [
                        NavigationRailDestination(icon: Icon(Icons.dashboard), label: Text('Dashboard')),
                        NavigationRailDestination(icon: Icon(Icons.restaurant_menu), label: Text('Menyu')),
                        NavigationRailDestination(icon: Icon(Icons.people), label: Text('Navbat')),
                        NavigationRailDestination(icon: Icon(Icons.people_alt), label: Text('Mijozlar')),
                        NavigationRailDestination(icon: Icon(Icons.history), label: Text('Tarix')),
                        NavigationRailDestination(icon: Icon(Icons.receipt), label: Text('Invoyslar')),
                        NavigationRailDestination(icon: Icon(Icons.settings), label: Text('Sozlamalar')),
                      ],
                    ),
                    const VerticalDivider(thickness: 1, width: 1),
                    Expanded(child: _pages[currentIndex]),
                  ],
                );
              }
              return _pages[currentIndex];
            },
          ),
          bottomNavigationBar: !isDesktop
              ? BottomNavigationBar(
                  type: BottomNavigationBarType.fixed,
                  currentIndex: currentIndex < 3 ? currentIndex : 3,
                  onTap: (index) {
                    if (index == 3) {
                      scaffoldKey.currentState?.openEndDrawer();
                    } else {
                      context.read<MainCubit>().changePage(index);
                    }
                  },
                  selectedItemColor: currentIndex < 3 ? const Color(0xFF0D6EFD) : Colors.black54,
                  unselectedItemColor: Colors.black54,
                  items: const [
                    BottomNavigationBarItem(icon: Icon(Icons.dashboard), label: 'Dashboard'),
                    BottomNavigationBarItem(icon: Icon(Icons.restaurant_menu), label: 'Menyu'),
                    BottomNavigationBarItem(icon: Icon(Icons.people), label: 'Navbat'),
                    BottomNavigationBarItem(icon: Icon(Icons.menu), label: 'Ko\'proq'),
                  ],
                )
              : null,
        );
      },
    );
  }

  Widget _buildDrawerItem(BuildContext context, int index, IconData icon, Color color, String title, int currentIndex) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
      child: ListTile(
        contentPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 8),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
        leading: Icon(icon, color: color, size: 28),
        title: Text(title, style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
        selected: currentIndex == index,
        selectedTileColor: AppTheme.primary.withOpacity(0.1),
        onTap: () {
          Navigator.pop(context);
          context.read<MainCubit>().changePage(index);
        },
      ),
    );
  }
}
