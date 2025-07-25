using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Identity.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(name: "identity");

            migrationBuilder.CreateTable(
                name: "modulos",
                schema: "identity",
                columns: table => new
                {
                    id_modulo = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_modulo = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    descripcion = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    activo = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modulos", x => x.id_modulo);
                }
            );

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "identity",
                columns: table => new
                {
                    id_rol = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_rol = table.Column<string>(
                        type: "character varying(50)",
                        maxLength: 50,
                        nullable: false
                    ),
                    descripcion = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    activo = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id_rol);
                }
            );

            migrationBuilder.CreateTable(
                name: "tipos_permisos",
                schema: "identity",
                columns: table => new
                {
                    id_tipo_permiso = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_permiso = table.Column<string>(
                        type: "character varying(30)",
                        maxLength: 30,
                        nullable: false
                    ),
                    codigo = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    categoria = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    descripcion = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_permisos", x => x.id_tipo_permiso);
                }
            );

            migrationBuilder.CreateTable(
                name: "usuarios",
                schema: "identity",
                columns: table => new
                {
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    email = table.Column<string>(
                        type: "character varying(150)",
                        maxLength: 150,
                        nullable: false
                    ),
                    password = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: false
                    ),
                    activo = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    fecha_creacion = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id_usuario);
                }
            );

            migrationBuilder.CreateTable(
                name: "permisos",
                schema: "identity",
                columns: table => new
                {
                    id_permiso = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rol = table.Column<Guid>(type: "uuid", nullable: false),
                    id_modulo = table.Column<Guid>(type: "uuid", nullable: false),
                    id_tipo_permiso = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permisos", x => x.id_permiso);
                    table.ForeignKey(
                        name: "FK_permisos_modulos_id_modulo",
                        column: x => x.id_modulo,
                        principalSchema: "identity",
                        principalTable: "modulos",
                        principalColumn: "id_modulo",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_permisos_roles_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_permisos_tipos_permisos_id_tipo_permiso",
                        column: x => x.id_tipo_permiso,
                        principalSchema: "identity",
                        principalTable: "tipos_permisos",
                        principalColumn: "id_tipo_permiso",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                schema: "identity",
                columns: table => new
                {
                    id_refresh_token = table.Column<Guid>(type: "uuid", nullable: false),
                    token = table.Column<string>(
                        type: "character varying(500)",
                        maxLength: 500,
                        nullable: false
                    ),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    device_id = table.Column<string>(
                        type: "character varying(100)",
                        maxLength: 100,
                        nullable: false
                    ),
                    device_name = table.Column<string>(
                        type: "character varying(200)",
                        maxLength: 200,
                        nullable: false
                    ),
                    platform = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false
                    ),
                    app_version = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: true
                    ),
                    user_agent = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<string>(
                        type: "character varying(45)",
                        maxLength: 45,
                        nullable: true
                    ),
                    last_used = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    expires_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    is_revoked = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    revoked_reason = table.Column<string>(
                        type: "character varying(255)",
                        maxLength: 255,
                        nullable: true
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id_refresh_token);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "identity",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "usuarios_roles",
                schema: "identity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    id_usuario = table.Column<Guid>(type: "uuid", nullable: false),
                    id_rol = table.Column<Guid>(type: "uuid", nullable: false),
                    fecha_asignacion = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    CreatedAt = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_roles_id_rol",
                        column: x => x.id_rol,
                        principalSchema: "identity",
                        principalTable: "roles",
                        principalColumn: "id_rol",
                        onDelete: ReferentialAction.Cascade
                    );
                    table.ForeignKey(
                        name: "FK_usuarios_roles_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalSchema: "identity",
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_permisos_id_modulo",
                schema: "identity",
                table: "permisos",
                column: "id_modulo"
            );

            migrationBuilder.CreateIndex(
                name: "IX_permisos_id_rol_id_modulo_id_tipo_permiso",
                schema: "identity",
                table: "permisos",
                columns: new[] { "id_rol", "id_modulo", "id_tipo_permiso" },
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_permisos_id_tipo_permiso",
                schema: "identity",
                table: "permisos",
                column: "id_tipo_permiso"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_DeviceId",
                schema: "identity",
                table: "refresh_tokens",
                column: "device_id"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                schema: "identity",
                table: "refresh_tokens",
                column: "expires_at"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Platform",
                schema: "identity",
                table: "refresh_tokens",
                column: "platform"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                schema: "identity",
                table: "refresh_tokens",
                column: "token",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "identity",
                table: "refresh_tokens",
                column: "id_usuario"
            );

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_DeviceId",
                schema: "identity",
                table: "refresh_tokens",
                columns: new[] { "id_usuario", "device_id" }
            );

            migrationBuilder.CreateIndex(
                name: "IX_tipos_permisos_codigo",
                schema: "identity",
                table: "tipos_permisos",
                column: "codigo",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                schema: "identity",
                table: "usuarios",
                column: "email",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_roles_id_rol",
                schema: "identity",
                table: "usuarios_roles",
                column: "id_rol"
            );

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_roles_id_usuario_id_rol",
                schema: "identity",
                table: "usuarios_roles",
                columns: new[] { "id_usuario", "id_rol" },
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "permisos", schema: "identity");

            migrationBuilder.DropTable(name: "refresh_tokens", schema: "identity");

            migrationBuilder.DropTable(name: "usuarios_roles", schema: "identity");

            migrationBuilder.DropTable(name: "modulos", schema: "identity");

            migrationBuilder.DropTable(name: "tipos_permisos", schema: "identity");

            migrationBuilder.DropTable(name: "roles", schema: "identity");

            migrationBuilder.DropTable(name: "usuarios", schema: "identity");
        }
    }
}
