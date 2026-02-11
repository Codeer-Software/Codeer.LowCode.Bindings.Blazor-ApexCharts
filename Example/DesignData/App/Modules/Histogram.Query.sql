SELECT 
    printf('%d-%d', (quantity / 10) * 10, ((quantity / 10) * 10) + 9) AS range, -- 販売数量の範囲 (カテゴリ)
    COUNT(*) AS frequency                                                    -- 頻度 (件数)
FROM sales
GROUP BY (quantity / 10)
ORDER BY (quantity / 10);
