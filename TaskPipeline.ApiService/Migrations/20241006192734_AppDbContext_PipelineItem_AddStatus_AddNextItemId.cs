using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskPipeline.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AppDbContext_PipelineItem_AddStatus_AddNextItemId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PipelineItem",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PipelineItem");
        }
    }
}
