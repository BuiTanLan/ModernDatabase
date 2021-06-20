import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShopComponent } from './shop.component';
import { ProductItemComponent } from './product-item/product-item.component';
import { SharedModule } from '../shared/shared.module';
import { ProductsDetailsComponent } from './products-details/products-details.component';
import { RouterModule } from '@angular/router';
import { ShopRoutingModule } from './shop-routing.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ProductCommentComponent } from './product-comment/product-comment.component';
import { NzCommentModule } from 'ng-zorro-antd/comment';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';
import { NzListModule } from 'ng-zorro-antd/list';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';



@NgModule({
  declarations: [
    ShopComponent,
    ProductItemComponent,
    ProductsDetailsComponent,
    ProductCommentComponent
  ],
  imports: [
    CommonModule,
    SharedModule,
    RouterModule,
    ShopRoutingModule,
    FormsModule,
    NzCommentModule,
    NzAvatarModule,
    NzListModule,
    NzButtonModule,
    NzFormModule,
    ReactiveFormsModule
  ],
})
export class ShopModule {}
