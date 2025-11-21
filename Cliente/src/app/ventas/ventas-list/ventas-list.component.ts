import { Component, OnInit } from '@angular/core';
import { VentasService } from '../../core/services/ventas.service';
import { VentaDto } from '../../core/models/dtos';

@Component({
    selector: 'app-ventas-list',
    templateUrl: './ventas-list.component.html',
    styleUrls: ['./ventas-list.component.scss']
})
export class VentasListComponent implements OnInit {
    ventas: VentaDto[] = [];

    constructor(private ventasService: VentasService) { }

    ngOnInit(): void {
        this.loadVentas();
    }

    loadVentas() {
        this.ventasService.getMisVentas().subscribe({
            next: (data) => {
                this.ventas = data;
            },
            error: (err) => {
                console.error('Error cargando ventas', err);
            }
        });
    }
}
