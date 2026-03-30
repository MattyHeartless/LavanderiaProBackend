# Guia Frontend - Servicios con Pricing Options y Desglose de Ropa

## Objetivo del cambio

Antes, cada servicio de catalogo tenia un solo `price` y un solo `uoM`. Eso no permitia que un mismo servicio (por ejemplo, Lavado y Secado) tuviera variantes de cobro como:

- Por kilo
- Por pieza
- Por docena
- Bulto pequeno
- Bulto mediano
- Bulto grande
- Bulto jumbo

Ahora, cada servicio tiene una coleccion de opciones de precio (`pricingOptions`).
Ademas, para opciones tipo bulto, el checkout debe separar cuantas prendas son de color y cuantas son negras.

## Catalogo valido de optionName y UoM

| optionName | UoM requerido |
|---|---|
| Por kilo | KG |
| Por pieza | PZ |
| Por docena | DOC |
| Bulto pequeño | BULTO |
| Bulto mediano | BULTO |
| Bulto grande | BULTO |
| Bulto jumbo | BULTO |

## Base URLs

- Catalogs: `http://localhost:5009/api/Catalogs`
- Orders: `http://localhost:5252/api/Orders`

---

## Endpoints: antes y despues

### 1) GET /api/Catalogs/services

### Antes
```json
{
  "services": [
    {
      "id": "uuid",
      "name": "Lavado y Secado",
      "description": "...",
      "price": 25.00,
      "uoM": "KG",
      "isActive": true,
      "icon": "...",
      "themeIcon": "..."
    }
  ]
}
```

### Despues
```json
{
  "services": [
    {
      "id": "uuid",
      "name": "Lavado y Secado",
      "description": "...",
      "price": 25.00,
      "uoM": "KG",
      "isActive": true,
      "icon": "...",
      "themeIcon": "...",
      "pricingOptions": [
        {
          "id": "uuid-opcion",
          "serviceId": "uuid",
          "optionName": "Por kilo",
          "price": 25.00,
          "uoM": "KG",
          "isActive": true,
          "createdAt": "2026-03-30T00:00:00Z",
          "updatedAt": "2026-03-30T00:00:00Z"
        },
        {
          "id": "uuid-opcion-2",
          "serviceId": "uuid",
          "optionName": "Bulto mediano",
          "price": 90.00,
          "uoM": "BULTO",
          "isActive": true,
          "createdAt": "2026-03-30T00:00:00Z",
          "updatedAt": "2026-03-30T00:00:00Z"
        }
      ]
    }
  ]
}
```

Nota frontend:
- Para mostrar precios al usuario, usar `pricingOptions`.
- `price` y `uoM` en la raiz del servicio quedan como legacy.

---

### 2) GET /api/Catalogs/services/{id}

### Antes
- Devolvia un solo servicio con `price` y `uoM`.

### Despues
- Devuelve el servicio incluyendo `pricingOptions`.

---

### 3) PUT /api/Catalogs/services/{id}

### Antes
- Se usaba tambien para actualizar `price` y `uoM` del servicio.

### Despues
- `price` y `uoM` ya no son la fuente de verdad para el checkout.
- Los precios se administran con endpoints de pricing options.

---

### 4) POST /api/Orders

### Antes
```json
{
  "order": { "...": "..." },
  "orderDetails": [
    {
      "serviceId": "uuid-como-string",
      "serviceName": "Lavado y Secado",
      "quantity": 3,
      "servicePrice": 25.00,
      "uoM": "KG"
    }
  ]
}
```

### Despues (con pricing option)
```json
{
  "order": { "...": "..." },
  "orderDetails": [
    {
      "serviceId": "uuid-como-string",
      "serviceName": "Lavado y Secado",
      "quantity": 3,
      "servicePrice": 25.00,
      "uoM": "KG",
      "servicePricingOptionId": "uuid-opcion",
      "pricingOptionName": "Por kilo"
    }
  ]
}
```

### Despues (si la opcion es bulto)
```json
{
  "order": { "...": "..." },
  "orderDetails": [
    {
      "serviceId": "uuid-como-string",
      "serviceName": "Lavado y Secado",
      "quantity": 5,
      "servicePrice": 90.00,
      "uoM": "BULTO",
      "servicePricingOptionId": "uuid-opcion-bulto",
      "pricingOptionName": "Bulto mediano",
      "coloredClothQuantity": 3,
      "blackClothQuantity": 2
    }
  ]
}
```

Regla importante:
- Cuando sea bulto, enviar `coloredClothQuantity` y `blackClothQuantity`.
- `quantity` debe seguir llegando y ser la suma: `quantity = coloredClothQuantity + blackClothQuantity`.

---

## Nuevos endpoints (Catalogs)

### GET /api/Catalogs/services/{serviceId}/pricing-options
Devuelve todas las opciones de precio del servicio.

### GET /api/Catalogs/pricing-options/{optionId}/is-active
Valida si una opcion sigue activa antes de crear la orden.

### POST /api/Catalogs/services/{serviceId}/pricing-options
Crea opcion de precio (admin).

### PUT /api/Catalogs/services/{serviceId}/pricing-options/{optionId}
Actualiza opcion de precio (admin).

### DELETE /api/Catalogs/services/{serviceId}/pricing-options/{optionId}
Elimina opcion de precio (admin), con regla de no dejar el servicio sin opciones activas.

---

## Reglas de negocio que frontend debe respetar

1. En checkout, mostrar solo `pricingOptions` activas (`isActive = true`).
2. Antes de confirmar orden, validar opcion con `GET /pricing-options/{optionId}/is-active`.
3. Para opciones bulto (`pricingOptionName` contiene "Bulto"), mandar:
   - `coloredClothQuantity`
   - `blackClothQuantity`
   - `quantity` como suma de ambos
4. Para opciones no bulto, usar flujo normal con `quantity`.
5. Mantener snapshot en orden: `servicePricingOptionId`, `pricingOptionName`, `servicePrice`, `uoM`.

---

## Razones de este diseño

- Permite multiples esquemas de cobro por servicio sin duplicar servicios.
- Evita breaking changes en ordenes historicas (campos nuevos son nullable).
- Da trazabilidad: la orden guarda snapshot exacto de la opcion elegida.
- Habilita validaciones de negocio para bultos (color vs negra) sin romper el flujo tradicional por cantidad.
