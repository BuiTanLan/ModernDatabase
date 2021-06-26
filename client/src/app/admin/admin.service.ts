import {HttpClient, HttpParams} from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { ProductFormValues } from '../shared/models/product';
import {IPagination} from "../shared/models/pagination";
import {map} from "rxjs/operators";
import {ShopParams} from "../shared/models/shopParams";

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  baseUrl = environment.apiUrl;
  shopParams = new ShopParams();

  constructor(private http: HttpClient) { }

  createProduct(product: ProductFormValues) {
    return this.http.post(this.baseUrl + 'products', product);
  }

  updateProduct(product: ProductFormValues, id: string) {
    return this.http.put(this.baseUrl + 'products/' + id, product);
  }

  deleteProduct(id: string) {
    return this.http.delete(this.baseUrl + 'products/' + id);
  }

    uploadImage(file: File, id: string) {
    const formData = new FormData();
    formData.append('photo', file, 'image.png');
    return this.http.put(this.baseUrl + 'products/' + id + '/photo', formData, {
      reportProgress: true,
      observe: 'events'
    });
  }

  deleteProductPhoto(photoId: number, productId: string) {
    return this.http.delete(this.baseUrl + 'products/' + productId + '/photo/' + photoId);
  }

  setMainPhoto(photoId: number, productId: string) {
    return this.http.post(this.baseUrl + 'products/' + productId + '/photo/' + photoId, {});
  }

  getProducts() {
    let params = new HttpParams();
    if (this.shopParams.brandName) {
      params = params.append('brandName', this.shopParams.brandName);
    }
    if (this.shopParams.typeName) {
      params = params.append('typeName', this.shopParams.typeName);
    }
    if (this.shopParams.search) {
      params = params.append('search', this.shopParams.search);
    }
    params = params.append('sort', this.shopParams.sort);
    params = params.append('pageIndex', this.shopParams.pageNumber.toString());
    params = params.append('pageSize', this.shopParams.pageSize.toString());
    return this.http
      .get<IPagination>(this.baseUrl + 'products/admin', {
        observe: 'response',
        params,
      })
      .pipe(
        map((response) => {
          return  response.body;
        })
      );
  }



  setShopParams(params: ShopParams): void {
    this.shopParams = params;
  }

  getShopParams() {
    return this.shopParams;
  }

}
