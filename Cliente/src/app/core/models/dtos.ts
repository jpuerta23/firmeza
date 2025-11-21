export interface DetalleVentaCreateSimpleDto {
  productoId: number;
  cantidad: number;
}

export interface VentaCreateDto {
  metodoPago: string;
  detalles: DetalleVentaCreateSimpleDto[];
}

export interface DetalleVentaDto {
  productoId: number;
  productoNombre: string;
  cantidad: number;
  precioUnitario: number;
  subtotal: number;
}

export interface VentaDto {
  id: number;
  fecha: string;
  clienteId: number;
  clienteNombre: string;
  metodoPago: string;
  total: number;
  detalles: DetalleVentaDto[];
}

export interface Producto {
  id: number;
  nombre: string;
  precio: number;
  stock: number;
}
