using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NexoFleet.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "asp_net_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_identification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_users", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_users_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "clients",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tax_identification = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_clients", x => x.id);
                    table.UniqueConstraint("ak_clients_company_id_id", x => new { x.company_id, x.id });
                    table.ForeignKey(
                        name: "fk_clients_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_periods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    starts_on = table.Column<DateOnly>(type: "date", nullable: false),
                    ends_on = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_periods", x => x.id);
                    table.UniqueConstraint("ak_payment_periods_company_id_id", x => new { x.company_id, x.id });
                    table.CheckConstraint("ck_payment_periods_dates", "\"ends_on\" >= \"starts_on\"");
                    table.ForeignKey(
                        name: "fk_payment_periods_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "fk_asp_net_user_claims_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "fk_asp_net_user_logins_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_roles",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_asp_net_user_roles_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "fk_asp_net_user_tokens_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    data = table.Column<string>(type: "jsonb", maxLength: 10000, nullable: true),
                    ip_address = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_audit_logs_asp_net_users_actor_user_id",
                        column: x => x.actor_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_audit_logs_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    employee_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    identity_document = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    hire_date = table.Column<DateOnly>(type: "date", nullable: false),
                    uses_own_vehicle = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_employees", x => x.id);
                    table.UniqueConstraint("ak_employees_company_id_id", x => new { x.company_id, x.id });
                    table.ForeignKey(
                        name: "fk_employees_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_employees_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "routes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    route_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    instructions = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    estimated_duration_minutes = table.Column<int>(type: "integer", nullable: true),
                    reference_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    reference_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    destination_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    destination_latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    destination_longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    origin_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    origin_latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    origin_longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_routes", x => x.id);
                    table.UniqueConstraint("ak_routes_company_id_id", x => new { x.company_id, x.id });
                    table.ForeignKey(
                        name: "fk_routes_clients_company_id_client_id",
                        columns: x => new { x.company_id, x.client_id },
                        principalTable: "clients",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_routes_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    related_entity_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    read_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    archived_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.id);
                    table.ForeignKey(
                        name: "fk_notifications_asp_net_users_recipient_user_id",
                        column: x => x.recipient_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notifications_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_notifications_employees_company_id_recipient_employee_id",
                        columns: x => new { x.company_id, x.recipient_employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_period_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    base_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    published_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    voided_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_reports", x => x.id);
                    table.UniqueConstraint("ak_payment_reports_company_id_id", x => new { x.company_id, x.id });
                    table.CheckConstraint("ck_payment_reports_base_amount", "\"base_amount\" >= 0");
                    table.ForeignKey(
                        name: "fk_payment_reports_employees_company_id_employee_id",
                        columns: x => new { x.company_id, x.employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_reports_payment_periods_company_id_payment_period_id",
                        columns: x => new { x.company_id, x.payment_period_id },
                        principalTable: "payment_periods",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    ownership_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    license_plate = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    manufacture_year = table.Column<int>(type: "integer", nullable: false),
                    color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    passenger_capacity = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approval_status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    approval_decision_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    approval_decided_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.UniqueConstraint("ak_vehicles_company_id_id", x => new { x.company_id, x.id });
                    table.ForeignKey(
                        name: "fk_vehicles_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicles_employees_company_id_owner_employee_id",
                        columns: x => new { x.company_id, x.owner_employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "route_schedules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shift = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_until = table.Column<DateOnly>(type: "date", nullable: true),
                    default_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    default_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_schedules", x => x.id);
                    table.UniqueConstraint("ak_route_schedules_company_id_id", x => new { x.company_id, x.id });
                    table.CheckConstraint("ck_route_schedules_effective_period", "\"effective_until\" IS NULL OR \"effective_until\" >= \"effective_from\"");
                    table.ForeignKey(
                        name: "fk_route_schedules_routes_company_id_route_id",
                        columns: x => new { x.company_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "route_stops",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sequence = table.Column<int>(type: "integer", nullable: false),
                    instructions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_stops", x => x.id);
                    table.ForeignKey(
                        name: "fk_route_stops_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "routes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_comments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    author_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_comments", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_comments_asp_net_users_author_user_id",
                        column: x => x.author_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_comments_payment_reports",
                        columns: x => new { x.company_id, x.payment_report_id },
                        principalTable: "payment_reports",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_report_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_report_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_payment_report_files_asp_net_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_payment_report_files_payment_reports",
                        columns: x => new { x.company_id, x.payment_report_id },
                        principalTable: "payment_reports",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_documents", x => x.id);
                    table.ForeignKey(
                        name: "fk_vehicle_documents_asp_net_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_documents_vehicles_company_id_vehicle_id",
                        columns: x => new { x.company_id, x.vehicle_id },
                        principalTable: "vehicles",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "route_schedule_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    route_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_schedule_assignments", x => x.id);
                    table.CheckConstraint("ck_route_schedule_assignments_valid_period", "\"valid_until\" IS NULL OR \"valid_until\" >= \"valid_from\"");
                    table.ForeignKey(
                        name: "fk_route_schedule_assignments_employees_company_id_employee_id",
                        columns: x => new { x.company_id, x.employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_route_schedule_assignments_route_schedules",
                        columns: x => new { x.company_id, x.route_schedule_id },
                        principalTable: "route_schedules",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_route_schedule_assignments_vehicles_company_id_vehicle_id",
                        columns: x => new { x.company_id, x.vehicle_id },
                        principalTable: "vehicles",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "route_schedule_days",
                columns: table => new
                {
                    route_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_route_schedule_days", x => new { x.route_schedule_id, x.day_of_week });
                    table.ForeignKey(
                        name: "fk_route_schedule_days_route_schedules_route_schedule_id",
                        column: x => x.route_schedule_id,
                        principalTable: "route_schedules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trips",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: true),
                    route_id = table.Column<Guid>(type: "uuid", nullable: true),
                    route_schedule_id = table.Column<Guid>(type: "uuid", nullable: true),
                    submitted_by_employee_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    service_date = table.Column<DateOnly>(type: "date", nullable: false),
                    agreed_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    final_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    started_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    cancellation_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    destination_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    destination_latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    destination_longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    origin_address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    origin_latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true),
                    origin_longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trips", x => x.id);
                    table.UniqueConstraint("ak_trips_company_id_id", x => new { x.company_id, x.id });
                    table.CheckConstraint("ck_trips_agreed_amount", "\"agreed_amount\" IS NULL OR \"agreed_amount\" >= 0");
                    table.CheckConstraint("ck_trips_final_amount", "\"final_amount\" IS NULL OR \"final_amount\" >= 0");
                    table.CheckConstraint("ck_trips_service_times", "\"completed_at_utc\" IS NULL OR \"started_at_utc\" IS NULL OR \"completed_at_utc\" >= \"started_at_utc\"");
                    table.ForeignKey(
                        name: "fk_trips_clients_company_id_client_id",
                        columns: x => new { x.company_id, x.client_id },
                        principalTable: "clients",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trips_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trips_employees_company_id_submitted_by_employee_id",
                        columns: x => new { x.company_id, x.submitted_by_employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trips_route_schedules_company_id_route_schedule_id",
                        columns: x => new { x.company_id, x.route_schedule_id },
                        principalTable: "route_schedules",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trips_routes_company_id_route_id",
                        columns: x => new { x.company_id, x.route_id },
                        principalTable: "routes",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "payment_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: true),
                    effect = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_items", x => x.id);
                    table.CheckConstraint("ck_payment_items_amount", "\"amount\" >= 0");
                    table.ForeignKey(
                        name: "fk_payment_items_payment_reports_company_id_payment_report_id",
                        columns: x => new { x.company_id, x.payment_report_id },
                        principalTable: "payment_reports",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_payment_items_trips_company_id_trip_id",
                        columns: x => new { x.company_id, x.trip_id },
                        principalTable: "trips",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trip_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assigned_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_assignments", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_assignments_asp_net_users_assigned_by_user_id",
                        column: x => x.assigned_by_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_assignments_employees_company_id_employee_id",
                        columns: x => new { x.company_id, x.employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_assignments_trips_company_id_trip_id",
                        columns: x => new { x.company_id, x.trip_id },
                        principalTable: "trips",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_trip_assignments_vehicles_company_id_vehicle_id",
                        columns: x => new { x.company_id, x.vehicle_id },
                        principalTable: "vehicles",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trip_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    storage_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    content_type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    size_in_bytes = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_files", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_files_asp_net_users_uploaded_by_user_id",
                        column: x => x.uploaded_by_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_files_trips_company_id_trip_id",
                        columns: x => new { x.company_id, x.trip_id },
                        principalTable: "trips",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_incidents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reported_by_employee_id = table.Column<Guid>(type: "uuid", nullable: false),
                    severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    incident_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_incidents", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_incidents_employees_company_id_reported_by_employee_id",
                        columns: x => new { x.company_id, x.reported_by_employee_id },
                        principalTable: "employees",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_incidents_trips_company_id_trip_id",
                        columns: x => new { x.company_id, x.trip_id },
                        principalTable: "trips",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_reviews",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    comments = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    reviewed_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_reviews", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_reviews_asp_net_users_reviewer_user_id",
                        column: x => x.reviewer_user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_trip_reviews_trips_company_id_trip_id",
                        columns: x => new { x.company_id, x.trip_id },
                        principalTable: "trips",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trip_status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    trip_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    current_status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    occurred_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_trip_status_history", x => x.id);
                    table.ForeignKey(
                        name: "fk_trip_status_history_trips_company_id_trip_id",
                        columns: x => new { x.company_id, x.trip_id },
                        principalTable: "trips",
                        principalColumns: new[] { "company_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_role_claims_role_id",
                table: "asp_net_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_claims_user_id",
                table: "asp_net_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_logins_user_id",
                table: "asp_net_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_user_roles_role_id",
                table: "asp_net_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "asp_net_users",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "ix_asp_net_users_company_id",
                table: "asp_net_users",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "asp_net_users",
                column: "normalized_user_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_actor_user_id_occurred_at_utc",
                table: "audit_logs",
                columns: new[] { "actor_user_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_company_id_occurred_at_utc",
                table: "audit_logs",
                columns: new[] { "company_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_logs_entity_type_entity_id_occurred_at_utc",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_clients_company_id_client_code",
                table: "clients",
                columns: new[] { "company_id", "client_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clients_company_id_tax_identification",
                table: "clients",
                columns: new[] { "company_id", "tax_identification" },
                unique: true,
                filter: "\"tax_identification\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_companies_tax_identification",
                table: "companies",
                column: "tax_identification",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_company_id_email",
                table: "employees",
                columns: new[] { "company_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_company_id_employee_code",
                table: "employees",
                columns: new[] { "company_id", "employee_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_company_id_identity_document",
                table: "employees",
                columns: new[] { "company_id", "identity_document" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_employees_user_id",
                table: "employees",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_company_id_recipient_employee_id",
                table: "notifications",
                columns: new[] { "company_id", "recipient_employee_id" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_recipient_user_id_status_created_at_utc",
                table: "notifications",
                columns: new[] { "recipient_user_id", "status", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_related_entity",
                table: "notifications",
                columns: new[] { "company_id", "related_entity_type", "related_entity_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_comments_author_user_id",
                table: "payment_comments",
                column: "author_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_comments_company_id_payment_report_id",
                table: "payment_comments",
                columns: new[] { "company_id", "payment_report_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_comments_payment_report_id_created_at_utc",
                table: "payment_comments",
                columns: new[] { "payment_report_id", "created_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_items_company_id_payment_report_id",
                table: "payment_items",
                columns: new[] { "company_id", "payment_report_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_items_company_id_trip_id",
                table: "payment_items",
                columns: new[] { "company_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_items_payment_report_id",
                table: "payment_items",
                column: "payment_report_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_items_trip_id",
                table: "payment_items",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_periods_company_id_code",
                table: "payment_periods",
                columns: new[] { "company_id", "code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_periods_company_id_starts_on_ends_on",
                table: "payment_periods",
                columns: new[] { "company_id", "starts_on", "ends_on" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_report_files_company_id_payment_report_id",
                table: "payment_report_files",
                columns: new[] { "company_id", "payment_report_id" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_report_files_storage_key",
                table: "payment_report_files",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_payment_report_files_uploaded_by_user_id",
                table: "payment_report_files",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_reports_company_id_employee_id_status",
                table: "payment_reports",
                columns: new[] { "company_id", "employee_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_payment_reports_company_id_payment_period_id_employee_id",
                table: "payment_reports",
                columns: new[] { "company_id", "payment_period_id", "employee_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_route_schedule_assignments_company_id_employee_id_valid_from",
                table: "route_schedule_assignments",
                columns: new[] { "company_id", "employee_id", "valid_from" });

            migrationBuilder.CreateIndex(
                name: "ix_route_schedule_assignments_company_id_route_schedule_id",
                table: "route_schedule_assignments",
                columns: new[] { "company_id", "route_schedule_id" });

            migrationBuilder.CreateIndex(
                name: "ix_route_schedule_assignments_company_id_vehicle_id",
                table: "route_schedule_assignments",
                columns: new[] { "company_id", "vehicle_id" });

            migrationBuilder.CreateIndex(
                name: "ix_route_schedule_assignments_period",
                table: "route_schedule_assignments",
                columns: new[] { "route_schedule_id", "valid_from", "valid_until" });

            migrationBuilder.CreateIndex(
                name: "ix_route_schedule_assignments_route_schedule_id",
                table: "route_schedule_assignments",
                column: "route_schedule_id",
                unique: true,
                filter: "\"valid_until\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_route_schedules_company_id_effective_from_effective_until",
                table: "route_schedules",
                columns: new[] { "company_id", "effective_from", "effective_until" });

            migrationBuilder.CreateIndex(
                name: "ix_route_schedules_company_id_route_id_status",
                table: "route_schedules",
                columns: new[] { "company_id", "route_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_route_stops_route_id_sequence",
                table: "route_stops",
                columns: new[] { "route_id", "sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_routes_client_id",
                table: "routes",
                column: "client_id");

            migrationBuilder.CreateIndex(
                name: "ix_routes_company_id_client_id",
                table: "routes",
                columns: new[] { "company_id", "client_id" });

            migrationBuilder.CreateIndex(
                name: "ix_routes_company_id_route_code",
                table: "routes",
                columns: new[] { "company_id", "route_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_assignments_assigned_by_user_id",
                table: "trip_assignments",
                column: "assigned_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_assignments_company_id_employee_id_assigned_at_utc",
                table: "trip_assignments",
                columns: new[] { "company_id", "employee_id", "assigned_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_assignments_company_id_trip_id",
                table: "trip_assignments",
                columns: new[] { "company_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_assignments_company_id_vehicle_id",
                table: "trip_assignments",
                columns: new[] { "company_id", "vehicle_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_assignments_trip_id",
                table: "trip_assignments",
                column: "trip_id",
                unique: true,
                filter: "\"ended_at_utc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_trip_files_company_id_trip_id",
                table: "trip_files",
                columns: new[] { "company_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_files_storage_key",
                table: "trip_files",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_trip_files_uploaded_by_user_id",
                table: "trip_files",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_incidents_company_id_reported_by_employee_id",
                table: "trip_incidents",
                columns: new[] { "company_id", "reported_by_employee_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_incidents_company_id_trip_id",
                table: "trip_incidents",
                columns: new[] { "company_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_incidents_trip_id_incident_at_utc",
                table: "trip_incidents",
                columns: new[] { "trip_id", "incident_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_reviews_company_id_trip_id",
                table: "trip_reviews",
                columns: new[] { "company_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_reviews_reviewer_user_id",
                table: "trip_reviews",
                column: "reviewer_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_trip_reviews_trip_id_reviewed_at_utc",
                table: "trip_reviews",
                columns: new[] { "trip_id", "reviewed_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_status_history_company_id_trip_id",
                table: "trip_status_history",
                columns: new[] { "company_id", "trip_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trip_status_history_trip_id_occurred_at_utc",
                table: "trip_status_history",
                columns: new[] { "trip_id", "occurred_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_company_id_client_id",
                table: "trips",
                columns: new[] { "company_id", "client_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_company_id_route_id",
                table: "trips",
                columns: new[] { "company_id", "route_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_company_id_route_schedule_id",
                table: "trips",
                columns: new[] { "company_id", "route_schedule_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_company_id_service_date_status",
                table: "trips",
                columns: new[] { "company_id", "service_date", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_company_id_submitted_by_employee_id",
                table: "trips",
                columns: new[] { "company_id", "submitted_by_employee_id" });

            migrationBuilder.CreateIndex(
                name: "ix_trips_company_id_trip_number",
                table: "trips",
                columns: new[] { "company_id", "trip_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_documents_company_id_vehicle_id",
                table: "vehicle_documents",
                columns: new[] { "company_id", "vehicle_id" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_documents_storage_key",
                table: "vehicle_documents",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_documents_uploaded_by_user_id",
                table: "vehicle_documents",
                column: "uploaded_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_documents_vehicle_id_type_expires_on",
                table: "vehicle_documents",
                columns: new[] { "vehicle_id", "type", "expires_on" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_company_id_approval_status_status",
                table: "vehicles",
                columns: new[] { "company_id", "approval_status", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_company_id_license_plate",
                table: "vehicles",
                columns: new[] { "company_id", "license_plate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_company_id_owner_employee_id",
                table: "vehicles",
                columns: new[] { "company_id", "owner_employee_id" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_owner_employee_id",
                table: "vehicles",
                column: "owner_employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asp_net_role_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_logins");

            migrationBuilder.DropTable(
                name: "asp_net_user_roles");

            migrationBuilder.DropTable(
                name: "asp_net_user_tokens");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "payment_comments");

            migrationBuilder.DropTable(
                name: "payment_items");

            migrationBuilder.DropTable(
                name: "payment_report_files");

            migrationBuilder.DropTable(
                name: "route_schedule_assignments");

            migrationBuilder.DropTable(
                name: "route_schedule_days");

            migrationBuilder.DropTable(
                name: "route_stops");

            migrationBuilder.DropTable(
                name: "trip_assignments");

            migrationBuilder.DropTable(
                name: "trip_files");

            migrationBuilder.DropTable(
                name: "trip_incidents");

            migrationBuilder.DropTable(
                name: "trip_reviews");

            migrationBuilder.DropTable(
                name: "trip_status_history");

            migrationBuilder.DropTable(
                name: "vehicle_documents");

            migrationBuilder.DropTable(
                name: "asp_net_roles");

            migrationBuilder.DropTable(
                name: "payment_reports");

            migrationBuilder.DropTable(
                name: "trips");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "payment_periods");

            migrationBuilder.DropTable(
                name: "route_schedules");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "routes");

            migrationBuilder.DropTable(
                name: "asp_net_users");

            migrationBuilder.DropTable(
                name: "clients");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}
