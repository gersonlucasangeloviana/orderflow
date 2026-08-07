using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderFlow.Infrastructure.Migrations
{
    [DbContext(typeof(OrderFlowDbContext))]
    [Migration("20260807011000_SeedBasicCatalog")]
    public partial class SeedBasicCatalog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OrderFlow.Infrastructure.Migrations.Sql.seed-basic-catalog.sql")
                ?? throw new InvalidOperationException("Embedded catalog seed script was not found.");
            using var reader = new StreamReader(stream);
            migrationBuilder.Sql(reader.ReadToEnd());
        }

        protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.Sql("DELETE FROM dbo.Products WHERE Sku IN ('NOTE-PRO-14','MON-UW-29','TEC-MEC-ABNT2','MOUSE-WL','HEADSET-USB','WEBCAM-FHD','SSD-NVME-1TB','HUB-USBC-7P');");
    }
}
