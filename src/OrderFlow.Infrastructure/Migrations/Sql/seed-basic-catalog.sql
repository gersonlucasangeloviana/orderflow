-- Dados mínimos para criar pedidos no ambiente local.
-- A inserção é idempotente por SKU e é executada pela migration SeedBasicCatalog.
MERGE INTO dbo.Products AS target
USING (VALUES
  (CAST('10000000-0000-0000-0000-000000000001' AS uniqueidentifier), N'Notebook Pro 14', N'NOTE-PRO-14', CAST(5499.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000002' AS uniqueidentifier), N'Monitor UltraWide 29', N'MON-UW-29', CAST(1899.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000003' AS uniqueidentifier), N'Teclado Mecânico', N'TEC-MEC-ABNT2', CAST(429.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000004' AS uniqueidentifier), N'Mouse Sem Fio', N'MOUSE-WL', CAST(199.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000005' AS uniqueidentifier), N'Headset USB', N'HEADSET-USB', CAST(349.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000006' AS uniqueidentifier), N'Webcam Full HD', N'WEBCAM-FHD', CAST(279.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000007' AS uniqueidentifier), N'SSD NVMe 1TB', N'SSD-NVME-1TB', CAST(499.90 AS decimal(18,2)), CAST(1 AS bit)),
  (CAST('10000000-0000-0000-0000-000000000008' AS uniqueidentifier), N'Hub USB-C', N'HUB-USBC-7P', CAST(239.90 AS decimal(18,2)), CAST(1 AS bit))
) AS source (Id, Name, Sku, Price, IsActive)
ON target.Sku = source.Sku
WHEN NOT MATCHED THEN INSERT (Id, Name, Sku, Price, IsActive)
VALUES (source.Id, source.Name, source.Sku, source.Price, source.IsActive);
