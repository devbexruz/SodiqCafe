import 'package:flutter_bloc/flutter_bloc.dart';
import 'package:equatable/equatable.dart';
import '../../domain/repositories/menu_repository.dart';
import '../../data/models/product_model.dart';

import '../../data/models/bonus_campaign_model.dart';

// --- Events ---
abstract class MenuEvent extends Equatable {
  const MenuEvent();
  @override
  List<Object> get props => [];
}

class LoadMenuEvent extends MenuEvent {
  final int cafeId;
  const LoadMenuEvent({required this.cafeId});
  @override
  List<Object> get props => [cafeId];
}

class ToggleBonusEvent extends MenuEvent {
  final int campaignId;
  final int cafeId;
  const ToggleBonusEvent({required this.campaignId, required this.cafeId});
  @override
  List<Object> get props => [campaignId, cafeId];
}

class DeleteBonusEvent extends MenuEvent {
  final int campaignId;
  final int cafeId;
  const DeleteBonusEvent({required this.campaignId, required this.cafeId});
  @override
  List<Object> get props => [campaignId, cafeId];
}

class AddProductFromTemplateEvent extends MenuEvent {
  final int templateId;
  final double customPrice;
  final int cafeId;
  const AddProductFromTemplateEvent({required this.templateId, required this.customPrice, required this.cafeId});
  @override
  List<Object> get props => [templateId, customPrice, cafeId];
}

class AddCustomProductEvent extends MenuEvent {
  final ProductModel product;
  final int cafeId;
  const AddCustomProductEvent({required this.product, required this.cafeId});
  @override
  List<Object> get props => [product, cafeId];
}

class EditProductEvent extends MenuEvent {
  final ProductModel product;
  final int cafeId;
  const EditProductEvent({required this.product, required this.cafeId});
  @override
  List<Object> get props => [product, cafeId];
}

class DeleteProductEvent extends MenuEvent {
  final int productId;
  final int cafeId;
  const DeleteProductEvent({required this.productId, required this.cafeId});
  @override
  List<Object> get props => [productId, cafeId];
}

class AddBonusCampaignEvent extends MenuEvent {
  final BonusCampaignModel campaign;
  final int cafeId;
  const AddBonusCampaignEvent({required this.campaign, required this.cafeId});
  @override
  List<Object> get props => [campaign, cafeId];
}

// --- States ---
abstract class MenuState extends Equatable {
  const MenuState();
  @override
  List<Object> get props => [];
}

class MenuInitial extends MenuState {}

class MenuLoading extends MenuState {}

class MenuLoaded extends MenuState {
  final List<ProductModel> products;
  final List<BonusCampaignModel> activeBonuses;
  final List<BonusCampaignModel> allBonuses;
  
  const MenuLoaded({required this.products, required this.activeBonuses, required this.allBonuses});
  @override
  List<Object> get props => [products, activeBonuses, allBonuses];
}

class MenuError extends MenuState {
  final String message;
  const MenuError({required this.message});
  @override
  List<Object> get props => [message];
}

// --- BLoC ---
class MenuBloc extends Bloc<MenuEvent, MenuState> {
  final MenuRepository menuRepository;

  MenuBloc({required this.menuRepository}) : super(MenuInitial()) {
    on<LoadMenuEvent>(_onLoadMenu);
    on<ToggleBonusEvent>(_onToggleBonus);
    on<DeleteBonusEvent>(_onDeleteBonus);
    on<AddProductFromTemplateEvent>(_onAddProductFromTemplate);
    on<AddCustomProductEvent>(_onAddCustomProduct);
    on<EditProductEvent>(_onEditProduct);
    on<DeleteProductEvent>(_onDeleteProduct);
    on<AddBonusCampaignEvent>(_onAddBonusCampaign);
  }

  Future<void> _onLoadMenu(LoadMenuEvent event, Emitter<MenuState> emit) async {
    emit(MenuLoading());
    
    final productsResult = await menuRepository.getProducts(event.cafeId);
    final bonusesResult = await menuRepository.getBonusCampaigns(event.cafeId);
    
    productsResult.fold(
      (failure) => emit(MenuError(message: failure)),
      (products) {
        bonusesResult.fold(
          (failure) => emit(MenuError(message: failure)),
          (bonuses) {
            final activeBonuses = bonuses.where((b) => b.isActive && b.isVisible).toList();
            emit(MenuLoaded(products: products, activeBonuses: activeBonuses, allBonuses: bonuses));
          },
        );
      },
    );
  }

  Future<void> _onToggleBonus(ToggleBonusEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.toggleBonusCampaign(event.campaignId);
    result.fold(
      (failure) {}, // Ignored error handling for brevity, could show snackbar via listener
      (_) {
        add(LoadMenuEvent(cafeId: event.cafeId)); // Reload menu
      }
    );
  }

  Future<void> _onDeleteBonus(DeleteBonusEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.deleteBonusCampaign(event.campaignId);
    result.fold(
      (failure) {},
      (_) {
        add(LoadMenuEvent(cafeId: event.cafeId)); // Reload menu
      }
    );
  }

  Future<void> _onAddProductFromTemplate(AddProductFromTemplateEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.addProductFromTemplate(event.templateId, event.customPrice, event.cafeId);
    result.fold(
      (failure) {},
      (_) => add(LoadMenuEvent(cafeId: event.cafeId))
    );
  }

  Future<void> _onAddCustomProduct(AddCustomProductEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.addCustomProduct(event.product, event.cafeId);
    result.fold(
      (failure) {},
      (_) => add(LoadMenuEvent(cafeId: event.cafeId))
    );
  }

  Future<void> _onEditProduct(EditProductEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.editProduct(event.product);
    result.fold(
      (failure) {},
      (_) => add(LoadMenuEvent(cafeId: event.cafeId))
    );
  }

  Future<void> _onDeleteProduct(DeleteProductEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.deleteProduct(event.productId);
    result.fold(
      (failure) {},
      (_) => add(LoadMenuEvent(cafeId: event.cafeId))
    );
  }

  Future<void> _onAddBonusCampaign(AddBonusCampaignEvent event, Emitter<MenuState> emit) async {
    final result = await menuRepository.addBonusCampaign(event.campaign, event.cafeId);
    result.fold(
      (failure) {},
      (_) => add(LoadMenuEvent(cafeId: event.cafeId))
    );
  }
}
