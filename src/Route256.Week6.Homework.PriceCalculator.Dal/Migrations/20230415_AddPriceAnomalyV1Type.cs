using FluentMigrator;

namespace Route256.Week5.Workshop.PriceCalculator.Dal.Migrations;
[Migration(20230415, TransactionBehavior.None)]
public class AddPriceAnomalyTypeV1 : Migration
{
    public override void Up()
    {
        const string sql = @"
DO $$
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM pg_type WHERE typname = 'price_anomalies_v1') THEN
            CREATE TYPE price_anomalies_v1 as
            (
                  id      bigint
                , good_id bigint
                , price   numeric(19, 5)
            );
        END IF;
    END
$$;";

        Execute.Sql(sql);
    }

    public override void Down()
    {
        const string sql = @"
DO $$
    BEGIN
        DROP TYPE IF EXISTS price_anomalies_v1;
    END
$$;";

        Execute.Sql(sql);
    }
}