import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IBrand } from '../shared/models/brand';
import { IPagination, Pagination } from '../shared/models/pagination';
import { IType } from '../shared/models/type';
import { map } from 'rxjs/operators';
import { ShopParams } from '../shared/models/shopParams';
import { IProduct } from '../shared/models/product';
import { of } from 'rxjs';
import { env } from 'process';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ShopService {
  baseUrl = environment.apiUrl;
  products: IProduct[] = [];
  brands: IBrand[] = [];
  types: IType[] = [];
  pagination = new Pagination();
  shopParams = new ShopParams();

  constructor(private http: HttpClient) {}

  getComments(productId: string) {
    return this.http.get(this.baseUrl + 'comment/' + productId);
  }

  postComments(content: string, productId: string) {
    return this.http.post(this.baseUrl + 'comment', { content, productId });
  }
  getBrand() {
    if (this.brands.length > 0) {
      return of(this.brands);
    }
    return this.http.get<IBrand[]>(this.baseUrl + 'products/brands').pipe(
      map(response => {
        this.brands = response;
        return response;
       })
    );
  }
  getType() {
    if (this.types.length > 0) {
      return of(this.types);
    }
    return this.http.get<IType[]>(this.baseUrl + 'products/types').pipe(
      map(response => {
        this.types = response;
        return response;
       })
    );
  }


  getProduct(id: string) {
    return this.http.get<IProduct>(this.baseUrl + 'products/' + id);
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
      .get<IPagination>(this.baseUrl + 'products', {
        observe: 'response',
        params,
      })
      .pipe(
        map((response) => {
          this.pagination = response.body;
          return this.pagination;
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
