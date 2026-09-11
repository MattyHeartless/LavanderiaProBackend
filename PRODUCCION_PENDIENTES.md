# Preparación del backend para producción

## Contexto

El backend está compuesto por estos microservicios:

- Auth API
- Profile API
- Catalogs API
- Orders API
- Notifications API

El archivo principal de Docker Compose está en `services/Docker-compose.yml`.

Los Dockerfiles ya construyen correctamente las imágenes. También están implementadas las migraciones relacionadas con disponibilidad de repartidores, notificaciones push y la cola de eventos de pedidos.

El Compose actual funciona para desarrollo/local, pero todavía requiere configuración antes de considerarse listo para producción.

## Objetivo

Preparar una configuración de producción segura y reproducible sin cambiar la lógica funcional existente.

El dominio y HTTPS serán configurados externamente por el responsable del despliegue.

## Tareas obligatorias

### 1. Crear configuración de producción

Crear una variante de Compose para producción, por ejemplo:

```text
services/Docker-compose.production.yml
```

Debe utilizar:

```yaml
ASPNETCORE_ENVIRONMENT: Production
```

No modificar el Compose de desarrollo para romper el flujo local.

### 2. Eliminar secretos del repositorio

Actualmente existen valores sensibles o de desarrollo dentro de archivos de configuración y Compose, incluyendo:

- contraseña de SQL Server;
- usuario de SQL Server;
- claves JWT;
- `Notifications__InternalApiKey`;
- claves VAPID de Web Push.

En producción deben llegar mediante variables de entorno o un gestor de secretos. No deben quedar escritas directamente en el repositorio.

Se recomienda utilizar nombres de variables como:

```text
ConnectionStrings__AuthConnection
ConnectionStrings__ProfileConnection
ConnectionStrings__CatalogsDb
ConnectionStrings__OrdersDb
ConnectionStrings__NotificationsDb
Jwt__Key
Jwt__Issuer
Jwt__Audience
InternalApi__Key
Notifications__InternalApiKey
Vapid__Subject
Vapid__PublicKey
Vapid__PrivateKey
```

No imprimir valores reales de secretos en logs ni en documentación.

### 3. Revisar las cadenas de conexión

El Compose actual apunta a:

```text
192.168.100.53,1433
```

Confirmar que el servidor donde correrán los contenedores pueda acceder a esa dirección. Si el despliegue será en la nube, reemplazarla por el host real de SQL Server.

Usar conexión cifrada en producción, por ejemplo con `Encrypt=True`, y revisar si `TrustServerCertificate` debe estar en `False` según el certificado disponible.

Bases utilizadas:

- `LavanderiaPro`
- `LavanderiaProProfile`
- `LavanderiaProCatalogs`
- `LavanderiaProOrders`
- `LavanderiaProNotifications`

### 4. Configurar JWT de forma consistente

Todos los servicios que validan tokens deben compartir la misma configuración de producción:

- Auth
- Profile
- Catalogs
- Orders
- Notifications

Verificar que coincidan `Issuer`, `Audience` y la clave de firma. La clave debe ser suficientemente larga y mantenerse únicamente como secreto.

Probar como mínimo:

1. Login de cliente.
2. Login de administrador.
3. Login de repartidor.
4. Acceso autorizado a endpoints protegidos.
5. Rechazo de tokens inválidos o expirados.

### 5. Configurar CORS

CORS debe permitir los orígenes reales de las aplicaciones frontend desplegadas:

- LavanderiaPro, cliente.
- LavanderiaProAdmin, administrador.
- LavanderiaProRecolectorApp, repartidor.

No confundir CORS con la comunicación entre microservicios. CORS aplica principalmente a las solicitudes realizadas desde el navegador.

Se recomienda leer los orígenes desde configuración, por ejemplo una lista separada por comas, en lugar de dejarlos codificados permanentemente en `Program.cs`.

Mantener los orígenes localhost únicamente en la configuración de desarrollo.

### 6. Revisar Notifications API

Notifications se comunica con Orders mediante:

```text
Notifications__BaseUrl
Notifications__InternalApiKey
```

En Compose, Orders puede usar el nombre interno del servicio:

```text
http://notifications-api:8080/api/Notifications/
```

Si Notifications se despliega por separado, usar su URL privada o pública correspondiente.

El puerto de Notifications no debería exponerse públicamente si únicamente lo consume Orders. Mantenerlo dentro de la red interna siempre que la plataforma lo permita.

Configurar en producción:

- `Vapid__Subject` con un `mailto:` válido;
- `Vapid__PublicKey`;
- `Vapid__PrivateKey`;
- `InternalApi__Key`.

No reutilizar claves temporales de desarrollo si ya se generaron credenciales definitivas.

### 7. Ejecutar migraciones de producción

Antes de levantar tráfico real, aplicar las migraciones en cada base de datos.

Migraciones recientes importantes:

- disponibilidad de repartidores en Catalogs;
- base y tabla de suscripciones push en Notifications;
- cola `OrderNotificationOutbox` en Orders.

Confirmar el estado con:

```bash
dotnet ef database update
```

Ejecutar el comando desde el proyecto correspondiente usando la cadena de conexión de producción. No depender de una migración automática al arrancar la aplicación sin revisar primero el procedimiento de despliegue.

### 8. Mejorar el Compose de producción

Agregar, según las capacidades del servidor:

- `restart: unless-stopped`;
- healthchecks para cada API;
- red privada explícita;
- límites de CPU y memoria;
- nombres de contenedor no conflictivos;
- logs con rotación;
- volúmenes únicamente donde sean necesarios.

Publicar externamente solo los servicios que deban recibir tráfico del navegador o del proxy inverso.

### 9. Proxy, HTTPS y encabezados

El dominio y HTTPS serán configurados externamente. El responsable del proxy debe enrutar correctamente hacia los servicios y conservar los encabezados necesarios, incluyendo:

- `X-Forwarded-For`;
- `X-Forwarded-Proto`;
- `X-Forwarded-Host`.

Confirmar que las APIs acepten solicitudes HTTPS detrás del proxy y que no se publiquen directamente puertos internos innecesarios.

### 10. Verificaciones finales

Después del despliegue, verificar:

- Swagger deshabilitado o protegido en producción.
- Login de cliente, admin y repartidor.
- Consulta y actualización de perfil.
- Catálogos y servicios.
- Creación de pedidos.
- Consulta de pedidos no asignados.
- Activación/desactivación de disponibilidad del repartidor.
- Registro de suscripción push.
- Recepción de notificación al crear un pedido sin asignar.
- CORS desde los tres frontends reales.
- Respuestas correctas para 401, 403, 404 y 500.
- Logs sin contraseñas, tokens ni claves privadas.

## Estado conocido del Compose actual

El archivo actual todavía contiene valores de desarrollo como:

- `ASPNETCORE_ENVIRONMENT=Development`;
- credenciales SQL escritas directamente;
- `192.168.100.53` como servidor fijo;
- clave interna de Notifications de desarrollo;
- orígenes CORS de localhost;
- configuración VAPID asociada al entorno de desarrollo.

Por lo tanto, el backend puede levantarse y probarse, pero no debe considerarse configuración final de producción hasta completar esta lista.

## Criterio de terminado

El trabajo estará terminado cuando:

1. Exista un Compose de producción separado.
2. Ningún secreto real esté versionado.
3. Todos los servicios arranquen con `Production`.
4. Las cinco bases tengan sus migraciones aplicadas.
5. Los tres frontends puedan consumir las APIs mediante sus dominios reales.
6. Orders pueda publicar eventos hacia Notifications.
7. Un repartidor disponible reciba una notificación push de un nuevo pedido.
8. Se hayan realizado pruebas de humo y no existan errores de configuración en los logs.

