import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductosListComponent } from './productos-list/productos-list.component';
import { ProductosRoutingModule } from './productos-routing.module';

@NgModule({
  declarations: [
    ProductosListComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    ProductosRoutingModule
  ],
  exports: []
})
export class ProductosModule { }
