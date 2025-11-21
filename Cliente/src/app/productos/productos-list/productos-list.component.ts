import { Component, OnInit } from '@angular/core';
import { ProductosService } from '../../core/services/productos.service';
import { VentasService } from '../../core/services/ventas.service';
import { Producto, VentaCreateDto } from '../../core/models/dtos';
import { Router } from '@angular/router';

@Component({
    selector: 'app-productos-list',
    templateUrl: './productos-list.component.html',
    styleUrls: ['./productos-list.component.scss']
})
export class ProductosListComponent implements OnInit {
    productos: Producto[] = [];

    constructor(
        private productosService: ProductosService,
        private ventasService: VentasService,
        private router: Router
    ) { }

    ngOnInit(): void {
        this.loadProductos();
    }

    loadProductos() {
        this.productosService.getProductos().subscribe({
            next: (data) => {
                this.productos = data;
            },
            error: (err) => {
                console.error('Error cargando productos', err);
            }
        });
    }

    // Modal state
    showModal = false;
    selectedProduct: Producto | null = null;
    quantity: number = 1;
    paymentMethod: string = 'Efectivo';
    paymentMethods: string[] = ['Efectivo', 'Tarjeta de Crédito', 'Transferencia'];

    openModal(producto: Producto) {
        this.selectedProduct = producto;
        this.quantity = 1;
        this.paymentMethod = 'Efectivo';
        this.showModal = true;
    }

    closeModal() {
        this.showModal = false;
        this.selectedProduct = null;
    }

    get total(): number {
        return this.selectedProduct ? this.selectedProduct.precio * this.quantity : 0;
    }

    confirmPurchase() {
        if (!this.selectedProduct) return;

        if (this.quantity > this.selectedProduct.stock) {
            alert('La cantidad supera el stock disponible');
            return;
        }

        const venta: VentaCreateDto = {
            metodoPago: this.paymentMethod,
            detalles: [
                {
                    productoId: this.selectedProduct.id,
                    cantidad: this.quantity
                }
            ]
        };

        this.ventasService.crearVenta(venta).subscribe({
            next: (res) => {
                alert('Compra realizada con éxito!');
                this.closeModal();
                this.loadProductos();
            },
            error: (err) => {
                console.error('Error al comprar', err);
                alert('Error al realizar la compra: ' + (err.error || 'Desconocido'));
            }
        });
    }
}
