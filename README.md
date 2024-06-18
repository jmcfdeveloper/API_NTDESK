# API para NTDESK

Esta API proporciona funcionalidades para interactuar con el sistema NTDESK. Permite generar PDFs a partir de URLs y llenar plantillas HTML con códigos de tickets.

## Endpoints

### Generar PDF desde URL
Genera un PDF a partir de una URL especificada.

#### Parámetros de solicitud
- `url` (string, requerido): La URL de la página web de la que se desea generar el PDF.

#### Respuestas
- `200 OK`: Se devuelve el PDF generado correctamente.
- `400 Bad Request`: La URL especificada no es válida.
- `500 Internal Server Error`: Error interno del servidor.

### Llenar Plantilla HTML
Llena una plantilla HTML con un código de ticket.

#### Parámetros de solicitud
El cuerpo de la solicitud debe contener un objeto JSON con el siguiente formato:
```json
{
  "TicketCode": "string"
}


