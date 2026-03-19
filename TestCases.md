### ProductServiceBatchInsertTests (3 теста)
Массовая вставка продуктов:
- (Positive) Вставка валидных продуктов 
- (Negative) Цена <= 0. Ожидание ValidationException
- (Negative) Пустое имя. Ожидание ValidationException

### ProductServiceBatchUpdateTests (3 теста)
Массовое обновление:
- (Positive) Валидные обновления
- (Negative) ID <= 0. Ожидание ValidationException
- (Negative) Отрицательное количество. Ожидание ValidationException

### ProductServiceBatchDeleteTests (3 теста)
Массовое удаление:
- (Positive) Корректное удаление по ID
- (Negative) ID < 0. Ожидание ValidationException
- (Positive) Удаление одного продукта

### ProductRepositoryTests (6 тестов)
InMemory БД:
- (Positive) Insert: добавление в БД
- (Positive) Get: получение по ID
- (Positive) Update: сохраняет CreatedAt
- (Positive) Batch Insert: вставка нескольких
- (Negative) Batch Update несуществующего. Ожидание KeyNotFoundException
- (Positive) Batch Update: сохраняет опциональные поля
