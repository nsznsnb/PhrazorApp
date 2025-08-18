SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    ------------------------------------------------------------
    -- 1) 操作種別マスタ (M_OPERATION_TYPES)
    ------------------------------------------------------------
    MERGE dbo.M_OPERATION_TYPES WITH (HOLDLOCK) AS tgt
    USING (VALUES
        (N'01', N'OpenAI画像生成', 20),
        (N'02', N'OpenAI文書添削',  5)
    ) AS src(operation_type_code, operation_type_name, operation_type_limit)
        ON tgt.operation_type_code = src.operation_type_code
    WHEN MATCHED THEN
        UPDATE SET
            tgt.operation_type_name  = src.operation_type_name,
            tgt.operation_type_limit = src.operation_type_limit,
            tgt.updated_at           = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (operation_type_id, operation_type_code, operation_type_name, operation_type_limit, created_at, updated_at)
        VALUES (NEWID(), src.operation_type_code, src.operation_type_name, src.operation_type_limit, GETDATE(), GETDATE());
    ;

    ------------------------------------------------------------
    -- 2) 成績マスタ (M_GRADES)
    ------------------------------------------------------------
    MERGE dbo.M_GRADES WITH (HOLDLOCK) AS tgt
    USING (VALUES
        (N'S', 1),
        (N'A', 2),
        (N'B', 3),
        (N'D', 4)
    ) AS src(grade_name, order_no)
        ON tgt.grade_name = src.grade_name
    WHEN MATCHED THEN
        UPDATE SET
            tgt.order_no   = src.order_no,
            tgt.updated_at = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (grade_id, grade_name, order_no, created_at, updated_at)
        VALUES (NEWID(), src.grade_name, src.order_no, GETDATE(), GETDATE());
    ;

    ------------------------------------------------------------
    -- 3) 復習種別マスタ (M_REVIEW_TYPES)
    --    追加する種別名：テストによる復習、カードによる復習
    ------------------------------------------------------------
    MERGE dbo.M_REVIEW_TYPES WITH (HOLDLOCK) AS tgt
    USING (VALUES
        (N'テストによる復習'),
        (N'カードによる復習')
    ) AS src(review_type_name)
        ON tgt.review_type_name = src.review_type_name
    WHEN MATCHED THEN
        UPDATE SET
            tgt.updated_at = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (review_type_id, review_type_name, created_at, updated_at)
        VALUES (NEWID(), src.review_type_name, GETDATE(), GETDATE());
    ;

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;

