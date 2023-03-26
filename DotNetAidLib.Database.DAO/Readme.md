## Documentación Dao

### Definición de Entidades Dao:

- A tener en cuenta:
    1. Todas las entidades a mapear tendrán su correspondiente clase de entidad.
        1. Las clases de entidades tendrán su atributo `[DaoEntity]` correspondiente
    2. Todos los atributos a mapear serán propiedades en su clase de entidad correspondiente.
        1. Las propiedades tendrán su atributo `[DaoPropertyX]` correspondiente.
        2. Las propiedades tendrán sus accesores get y set.
        3. Las propiedades serán del tipo acorde a su tipo de datos en DB.
        4. Las propiedades serán de tipo 'virtual'.
        5. Cada entidad debe de tener al menos una o varias propiedades de tipo PK.
    3. Todas las propiedades de navegación 1-N serán del tipo ICollection.

### Definición de Atributos Dao:

- Atributos de entidad O-R:
  `[DaoEntity]` Atributo que especifica que esta clase es una entidad.
  tableName:    Nombre de tabla en DB, por omisión 'igual al nombre de la clase'.

- Atributos de propiedades de entidad O-R:
  `[DaoProperty]` Propiedad de la entidad a establecer
  columnName Nombre de la columna en la tabla, por omisión 'igual al nombre de la propiedad'
  length Longitud del campo en la columna de la tabla, si es el caso.
  mode Modo de establecimiento del valor de propiedad: 'Auto' siempre / 'Manual' bajo demanda, por omisión 'Auto'
  `[DaoPropertyPK]` Propiedad de tipo Clave Primaria
  order En caso de ser una PK compuesta por varias propiedades PK, el orden de esta propiedad con respecto a las demás,
  por omisión '0'.
  `[DaoPropertyFK]` Propiedad de tipo Clave Ajena
  order En caso de ser una FK compuesta por varias propiedades FK, el orden de esta propiedad con respecto a las demás,
  por omisión '0'.

- Atributos de generadores de valores de PK para entidades Dao:
  `[DaoPropertyPKGeneratorGUID]`        PK de tipo GUID generado por el Dao antes de la inserción.
  `[DaoPropertyPKGeneratorIdentity]`    PK de tipo autonumérico generado por la DB después de la inserción.
  `[DaoPropertyPKGeneratorSequence]`    PK de tipo secuencia generado por la DB antes de la inserción.
  sequenceName Nombre de la secuencia en la DB.

- Atributos parseadores de tipos TipoNativo<->TipoDB para propiedades Dao:
  `[DaoPropertyParserEnum]` Atributo parseador para propiedades de tipo Enum.
  enumParseType Tipo de enumerado a serializar en la columna de la tabla: 'Integer' Integer / 'String' Nombre del
  enumerado en forma varchar(x), por omisión 'String'.
  `[DaoPropertyParserGUID]` Atributo parseador para propiedades de tipo GUID.
  guidParseType Tipo de GUID a serializar en la columna de la tabla: 'Binary' 16 bytes / 'String' GUID de tipo varchar(
  20), por omisión 'Binary'.
  `[DaoPropertyParserBlob]` Atributo parseador para propiedades de tipo DaoBlob (equivalentes a byte[] con métodos de
  lectura/guardado a archivo).
  `[DaoPropertyParserBinary]` Atributo parseador para propiedades de tipo byte[].
  `[DaoPropertyParserLocalBinary]` Atributo parseador para propiedades de tipo byte[] a archivos en una carpeta local
  con nombre igual al Id de la entidad.
  localBinaryFolder Ruta a la carpeta local donde se encuentran los binarios, por omisión el valor de la variable de
  contexto 'localBinaryFolder'.
  `[DaoPropertyParserStringCrypt]` Atributo parseador para propiedades de tipo String que se guardan encriptadas AES en
  el campo de la db.
  passPhrase Clave de la encriptación.
  salt Tipo de sal para la encriptación, por defecto 'dsfPOASTdg54whdykt_dwFD'.

- Atributos de tipo Navegación:
  `[DaoNavigationProperty1ToN]` Atributos de Navegación 1-N por FK (relación 1-N, teniendo N una propiedad con
  atributo `[DaoNavigationPropertyFK]` que enlaza con esta)
  Tipo propiedad: ICollection`<T>`
  Atributo en la entidad N: `[DaoNavigationPropertyFK]`
  `[DaoNavigationProperty1To1FK]` Atributos de Navegación 1-1 por FK (igual a atributo 1-N, donde N es único).
  Tipo propiedad: T
  Atributo en la entidad N: `[DaoNavigationPropertyFK]`
  `[DaoNavigationProperty1To1PK]` Atributos de Navegación 1-1 por PKs (relación 1-1, donde las PKs son las mismas).
  Tipo propiedad: T
  Atributo en la otra entidad 1: `[DaoNavigationPropertyPK]`

  `[DaoNavigationPropertyFK]` Atributo de navegación hacia atrás en las navegaciónes 1ToN, 1To1FK y 1To1PK.
  ReferencePropertyName Nombre de la propiedad en el otro lado que enlaza con esta.
  ForeignKeyProperty/ies Nombre/s de propiedad/es de esta entidad que son clave/s foránea/s de esta relación (en su
  orden en caso de ser clave compuesta)

~~~
### Ejemplos definición Dao
Ejemplo 1-N:
	A										B
	================================================================
	Id PK									Id PK
										AId FK

	[DaoNavigationProperty1ToN]				[DaoNavigationPropertyFK(ReferencePropertyName="AId", ForeignKeyProperty="Bs")]
	ICollection<BType> Bs					BType A 

Ejemplo 1-1PK:
	A										B
	================================================================
	Id PK									Id PK

	[DaoNavigationProperty1To1PK]			[DaoNavigationPropertyFK(ReferencePropertyName="Id", ForeignKeyProperty="B")]
	BType B									AType A

Ejemplo 1-1FK:
	A										B
	================================================================
	Id PK									Id PK
											AId FK

	[DaoNavigationProperty1To1FK]			[DaoNavigationPropertyFK(ReferencePropertyName="AId", ForeignKeyProperty="B")]
	BType B									BType A
~~~

## Objetos Dao

DBProviderConnector Objeto que representa un conector hacia una base de datos.
proveedor de datos.

DaoContext Objeto de contexto Dao hacia un conector.
