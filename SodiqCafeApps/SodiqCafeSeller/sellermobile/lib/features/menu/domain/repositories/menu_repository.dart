import 'package:dartz/dartz.dart';
import '../../data/models/product_model.dart';
import '../../data/models/bonus_campaign_model.dart';

abstract class MenuRepository {
  Future<Either<String, List<ProductModel>>> getProducts(int cafeId);
  Future<Either<String, List<ProductModel>>> getTemplates();
  Future<Either<String, ProductModel>> addProductFromTemplate(int templateId, double customPrice, int cafeId);
  Future<Either<String, ProductModel>> addCustomProduct(ProductModel product, int cafeId);
  Future<Either<String, ProductModel>> editProduct(ProductModel product);
  Future<Either<String, bool>> deleteProduct(int productId);

  Future<Either<String, List<BonusCampaignModel>>> getBonusCampaigns(int cafeId);
  Future<Either<String, BonusCampaignModel>> addBonusCampaign(BonusCampaignModel campaign, int cafeId);
  Future<Either<String, bool>> toggleBonusCampaign(int campaignId);
  Future<Either<String, bool>> deleteBonusCampaign(int campaignId);
}
