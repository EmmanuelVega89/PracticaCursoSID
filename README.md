# Guía de uso - API REST SID CFE-LAPEM

## Introducción
La API REST del **Sistema de Inspección a Distancia (SID)** de CFE-LAPEM permite a las empresas fabricantes interactuar con los servicios de inspección, fabricación, pruebas y liberación.  
Este documento tiene como objetivo servir como **guía de capacitación** para el uso correcto de los endpoints, el manejo de autenticación y el flujo general.

---

## Requisitos Previos
- Conexión a internet.
- Herramienta de pruebas de APIs (ej. [Postman](https://www.postman.com/) o `curl`).
- Credenciales de acceso (usuario/contraseña).
- Token de autorización tipo **Bearer (JWT)**.

### Autenticación
Todos los servicios requieren autenticación mediante **Bearer Token** en el encabezado HTTP:

```http
Authorization: Bearer <tu_token>
```

Para obtener el token se debe invocar el endpoint de **Login**.

---

## Servidor Base
```
/sid_capacitacion
```

Todos los endpoints inician con este prefijo.

---

## Endpoints Principales

### 1. Acceso (F0_Acceso)

#### 🔑 Login
`POST /F0_Acceso/Login`  
Autentica al usuario y devuelve el token.

**Body (JSON):**
```json
{
  "username": "usuario",
  "password": "contraseña"
}
```

---

#### ⚙️ Configuración Empresa
- `POST /F0_Acceso/Configuracion` → Registrar configuración de empresa.
- `GET /F0_Acceso/Configuracion` → Consultar configuración actual.

---

### 2. Configuración Inicial (F1_ConfiguracionInicial)

- `POST /EstadoSID` → Reportar estado del SID (cada 60s).  
- `GET /Instrumento` → Listar instrumentos.  
- `POST /Instrumento` → Registrar instrumento.  
- `PUT /Instrumento` → Actualizar instrumento.  
- `GET /Prototipo` / `POST /Prototipo` / `PUT /Prototipo` → Manejo de prototipos.  
- `GET /Producto` / `POST /Producto` / `PUT /Producto` → Manejo de productos.  
- `GET /Norma` / `POST /Norma` / `PUT /Norma` → Manejo de normas.  
- `GET /Prueba` / `POST /Prueba` / `PUT /Prueba` → Manejo de pruebas.  
- `POST /DescripcionesCortas` → Consultar catálogos (paginado).

---

### 3. Preparación y Fabricación (F2_PreparacionFabricacion)

- `GET /Contratos` / `POST /Contratos` / `PUT /Contratos` → Manejo de contratos.  
- `POST /OrdenFabricacion` / `PUT /OrdenFabricacion` → Alta/actualización de órdenes de fabricación.  
- `GET /OrdenFabricacion/{Id}` → Consultar orden de fabricación.  
- `POST /ExpedientePruebas` / `PUT /ExpedientePruebas` → Manejo de expedientes.  
- `GET /ExpedientePruebas` → Listado paginado de expedientes.  
- `PUT /AgregaMuestraExpediente/{Expediente}` → Añadir muestra.  
- `PUT /QuitarMuestraExpediente/{Expediente}/{Muestra}` → Eliminar muestra.

---

### 4. Pruebas (F3_Pruebas)

- `PUT /AgregaResultadoPrueba` → Agregar resultado de prueba.  
- `GET /PruebasNoSatisfactorias/{Expediente}` → Consultar pruebas no satisfactorias.  
- `GET /ValidacionExpediente/{Expediente}` → Validar expediente antes de concluir.  
- `PUT /TerminarPruebasExpediente/{Expediente}` → Concluir pruebas.  
- `GET /ConsultaExpediente/{Expediente}` → Estado del expediente en LAPEM.

---

### 5. Liberación (F4_Liberacion)

- `GET /PrevioAvisosPrueba/{Expediente}` → Previo de avisos de prueba.  
- `PUT /CrearAvisoPrueba/{Expediente}` → Generar aviso de prueba.  
- `GET /ConsultaAvisos/{Expediente}` → Consultar avisos generados.

---

### 6. Servicios de Soporte

- `POST /Documento` / `GET /Documento` / `PUT /Documento` / `DELETE /Documento/{id}` → Manejo de documentos.  
- `POST /Aplicacion` / `GET /Aplicacion` / `PUT /Aplicacion` / `DELETE /Aplicacion/{id}` → Manejo de aplicaciones.

---

## Ejemplo de Flujo de Uso

1. **Autenticarse** con `/F0_Acceso/Login` y obtener token.  
2. **Registrar configuración** e instrumentos con `/F0_Acceso/Configuracion` y `/F1_ConfiguracionInicial/Instrumento`.  
3. **Dar de alta orden de fabricación** con `/F2_PreparacionFabricacion/OrdenFabricacion`.  
4. **Crear expediente de pruebas** con `/F2_PreparacionFabricacion/ExpedientePruebas`.  
5. **Agregar muestras y resultados de pruebas** con `/AgregaMuestraExpediente` y `/AgregaResultadoPrueba`.  
6. **Validar expediente** y concluir pruebas.  
7. **Generar aviso de prueba** y consultar en `/F4_Liberacion/ConsultaAvisos`.  

---

## Recomendaciones
- Usar entornos de prueba antes de producción.  
- Respetar los tiempos de reporte (`EstadoSID` cada 60s).  
- Mantener actualizado el catálogo de instrumentos y normas.  
- Documentar las respuestas de error de la API para depuración.  

---
## Estructura de DTOs de Contratos (ejemplos)
- Contrato CFE
```json
{
  "$Tipo": "ContratoCFE",
  "id": "68a7875adf255f466e564671",
  "tipoContrato": "ContratoCFE",
  "noContrato": "6875686",
  "estatus": "ACTIVO",
  "detalleContrato" : [{
        "partidaContrato" : "1",
        "descripcionAviso" : "Transformador",
        "areaDestinoCFE" : "CFE",
        "cantidad" : 10,
        "unidad": "PIEZA",
        "importeTotal": 1000
  }],
  "urlArchivo" : "https://www.cfe.mx",
  "mD5": "",
  "fechaEntregaCFE":"2025-07-18T17:25:06.51Z"
}
```
- Contrato CFE con garantía
```json
{
  "$Tipo": "ContratoCFEConGarantia",
  "id": "",
  "tipoContrato": "ContratoCFEConGarantia",
  "noContrato": "6875686",
  "estatus": "ACTIVO",
  "detalleContrato" : [{
        "partidaContrato" : "1",
        "descripcionAviso" : "Transformador",
        "areaDestinoCFE" : "CFE",
        "cantidad" : 100,
        "unidad": "PIEZA",
        "importeTotal": 1000
  }],
  "perdidasGarantizadasVacio": 2131.90,
  "perdidasGarantizadasCarga": 23.90,
  "urlArchivo" : "https://www.cfe.mx",
  "mD5": "",
  "fechaEntregaCFE":"2025-07-18T17:25:06.51Z"
}

```  
- Contrato Particular  
```json
{
  "$Tipo": "ContratoParticular",
  "id": "",
  "tipoContrato": "ContratoParticular",
  "noContrato": "PART0001",
  "estatus": "ACTIVO",
  "detalleContrato" : [{
        "partidaContrato" : "1",
        "descripcionAviso" : "Transformador",
        "cantidad" : 100,
        "unidad": "PIEZA",
        "importeTotal": 1000
  }]
}
```

---

## Contacto y Soporte
Para dudas o incidencias: [Soporte Técnico SID](mailto:soportelapem@cfe.mx)

---
