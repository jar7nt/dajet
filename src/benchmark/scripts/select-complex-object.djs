
DECLARE @Код string = '000000001'

DECLARE @Таблица array
DECLARE @Объект  object

USE 'mssql://zhichkin/dajet-metadata'
  
  SELECT TOP 1 Ссылка, Код, Наименование, ПометкаУдаления
    INTO @Объект
    FROM Справочник.Номенклатура
   WHERE Код = @Код

  SELECT TOP 3 Период, Цена
    INTO @Таблица
    FROM РегистрСведений.ЦеныНоменклатуры
   WHERE Номенклатура = @Объект.Ссылка
   ORDER BY Период DESC

END

MODIFY @Объект SELECT Таблица = @Таблица

RETURN JSON(@Объект)