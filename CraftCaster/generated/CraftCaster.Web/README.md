# CraftCaster.Web

El TDM define el dominio y los contratos. El proyecto conserva su asociación en appsettings.Haria.json.

- El Controller recibe datos, valida ModelState y llama a Application.
- El Assembler convierte entidades y ViewModels sin hacer persistencia.
- La cookie autentica; la sesión conserva datos auxiliares. Authorize protege las acciones; el menú solo presenta enlaces.
- La interfaz usa es-ES y coma decimal. El dato persistido y JSON siguen siendo numéricos.

## Organización

Controllers contiene los parciales principales y Controllers/manual su único complemento editable.
ViewModels, Assemblers y Views conservan su nivel. Utils agrupa Files, Localization y Security.
La portada incluye Views/Shared/_BusinessNavigation.cshtml: actualiza sus enlaces con Update Home Page y conserva los tuyos en su región manual.

## Sesión de cualquier clase

Importa CraftCaster.Web.Utils.Security y utiliza Session.SetClass(model), Session.GetClass<TViewModel>() y Session.ClearClass<TViewModel>().
Cada tipo tiene su propia clave. Usa ViewModels separados del dominio; el helper excluye contraseñas, hashes, tokens, archivos y grafos de entidades.
Los métodos SetUser/GetUser se conservan para los snapshots de acceso anteriores. Un snapshot no concede permisos.

## Autenticación y edición

Inicia la aplicación con HTTPS para utilizar su cookie segura. Register es registro propio y emite la cookie después de guardar; Create es alta administrativa y conserva la identidad del operador.
Los roles compartidos por una clase de usuario se configuran expresamente en HariaWeb:Authentication:RolesByUserClass (por defecto vacío). Por ejemplo, Administrador: ["Administrador"] permite utilizar [Authorize(Roles = "Administrador")]. Para roles por cuenta, utiliza serverEntity en el punto parcial AddTrustedClaims de HariaWebAuthentication. Protege las altas y modificaciones de cuentas privilegiadas con su política de roles. Tras cambiar roles, cierra y vuelve a iniciar sesión para renovar la cookie. El nombre de una clase por sí solo no concede un rol.
Las firmas y los atributos HTTP de acciones gestionadas se regeneran. Activa por separado las regiones attributes y body que quieras conservar. Las acciones enteramente manuales se mantienen.
Los campos fuera de un contrato de creación/edición no aparecen como inputs de esa operación; revisa los diagnósticos del generador.

## Cultura y archivos

El perfil predeterminado fija es-ES. Para habilitar varios idiomas configura explícitamente HariaWeb:Localization:EnableCultureSelection y SupportedCultures.
Los filtros GET localizados pueden declarar HariaLocalizedNumber; las rutas técnicas y JSON conservan su formato invariante.
Los uploads PNG/JPEG se guardan bajo wwwroot/uploads/Clase/Atributo/Id y son públicos. Una ruta de archivo recibida del navegador no sustituye la referencia conocida por el servidor.
