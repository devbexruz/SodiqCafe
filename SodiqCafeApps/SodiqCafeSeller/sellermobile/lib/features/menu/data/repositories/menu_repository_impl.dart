import 'package:dartz/dartz.dart';
import 'package:dio/dio.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/repositories/menu_repository.dart';
import '../models/product_model.dart';
import '../models/bonus_campaign_model.dart';

class MenuRepositoryImpl implements MenuRepository {
  final ApiClient apiClient;

  MenuRepositoryImpl({required this.apiClient});

  @override
  Future<Either<String, List<ProductModel>>> getProducts(int cafeId) async {
    try {
      final response = await apiClient.dio.get('/owner/products?cafeId=$cafeId');
      
      // Response datani parse qilish (ApiOk ishlatsa, data ichida keladi, yo'qsa to'g'ridan to'g'ri)
      final rawData = response.data;
      final List<dynamic> dataList = rawData['data'] ?? rawData;
      
      final products = dataList.map((json) => ProductModel.fromJson(json)).toList();
      return Right(products);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final message = e.response?.data['message'] ?? 'Xatolik yuz berdi';
        return Left(message);
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, List<BonusCampaignModel>>> getBonusCampaigns(int cafeId) async {
    try {
      final response = await apiClient.dio.get('/owner/bonus-campaigns?cafeId=$cafeId');
      
      final rawData = response.data;
      final List<dynamic> dataList = rawData['data'] ?? rawData;
      
      final campaigns = dataList.map((json) => BonusCampaignModel.fromJson(json)).toList();
      return Right(campaigns);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final message = e.response?.data['message'] ?? 'Xatolik yuz berdi';
        return Left(message);
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, bool>> toggleBonusCampaign(int campaignId) async {
    try {
      await apiClient.dio.post('/owner/bonus-campaigns/toggle', data: {
        'campaignId': campaignId
      });
      return const Right(true);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final message = e.response?.data['message'] ?? 'Xatolik yuz berdi';
        return Left(message);
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, bool>> deleteBonusCampaign(int campaignId) async {
    try {
      await apiClient.dio.post('/owner/bonus-campaigns/delete', data: {
        'campaignId': campaignId
      });
      return const Right(true);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        final message = e.response?.data['message'] ?? 'Xatolik yuz berdi';
        return Left(message);
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }
  @override
  Future<Either<String, List<ProductModel>>> getTemplates() async {
    try {
      final response = await apiClient.dio.get('/owner/products/templates');
      final rawData = response.data;
      final List<dynamic> dataList = rawData['data'] ?? rawData;
      
      final products = dataList.map((json) => ProductModel.fromJson(json)).toList();
      return Right(products);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        return Left(e.response?.data['message'] ?? 'Xatolik yuz berdi');
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, ProductModel>> addProductFromTemplate(int templateId, double customPrice, int cafeId) async {
    try {
      final response = await apiClient.dio.post('/owner/products/add-from-template', data: {
        'templateId': templateId,
        'customPrice': customPrice,
        'cafeId': cafeId,
      });
      final rawData = response.data;
      final data = rawData['data'] ?? rawData;
      return Right(ProductModel.fromJson(data));
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        return Left(e.response?.data['message'] ?? 'Xatolik yuz berdi');
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, ProductModel>> addCustomProduct(ProductModel product, int cafeId) async {
    try {
      final response = await apiClient.dio.post('/owner/products/add', data: {
        'name': product.name,
        'price': product.price,
        'description': product.description,
        'isRecommended': product.isRecommended,
        'category': product.category,
        'cafeId': cafeId,
      });
      final rawData = response.data;
      final data = rawData['data'] ?? rawData;
      return Right(ProductModel.fromJson(data));
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        return Left(e.response?.data['message'] ?? 'Xatolik yuz berdi');
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, ProductModel>> editProduct(ProductModel product) async {
    try {
      final response = await apiClient.dio.post('/owner/products/edit', data: {
        'productId': product.id,
        'name': product.name,
        'price': product.price,
        'description': product.description,
        'isRecommended': product.isRecommended,
        'category': product.category,
      });
      final rawData = response.data;
      final data = rawData['data'] ?? rawData;
      return Right(ProductModel.fromJson(data));
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        return Left(e.response?.data['message'] ?? 'Xatolik yuz berdi');
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, bool>> deleteProduct(int productId) async {
    try {
      await apiClient.dio.post('/owner/products/delete', data: {
        'productId': productId,
      });
      return const Right(true);
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        return Left(e.response?.data['message'] ?? 'Xatolik yuz berdi');
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }

  @override
  Future<Either<String, BonusCampaignModel>> addBonusCampaign(BonusCampaignModel campaign, int cafeId) async {
    try {
      final data = campaign.toJson();
      data['cafeId'] = cafeId;
      final response = await apiClient.dio.post('/owner/bonus-campaigns/add', data: data);
      final rawData = response.data;
      final responseData = rawData['data'] ?? rawData;
      return Right(BonusCampaignModel.fromJson(responseData));
    } on DioException catch (e) {
      if (e.response != null && e.response?.data != null) {
        return Left(e.response?.data['message'] ?? 'Xatolik yuz berdi');
      }
      return Left(e.message ?? 'Tarmoq xatosi');
    } catch (e) {
      return Left(e.toString());
    }
  }
}
