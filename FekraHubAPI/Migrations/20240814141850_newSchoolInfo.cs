using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FekraHubAPI.Migrations
{
    /// <inheritdoc />
    public partial class newSchoolInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractPages",
                table: "SchoolInfos");

            migrationBuilder.DropColumn(
                name: "StudentsReportsKeys",
                table: "SchoolInfos");

            migrationBuilder.AlterColumn<string>(
                name: "UrlDomain",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolOwner",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolName",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PrivacyPolicy",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LogoBase64",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FromEmail",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "EmailServer",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateTable(
                name: "contractPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConPage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolInfoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contractPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_contractPages_SchoolInfos_SchoolInfoId",
                        column: x => x.SchoolInfoId,
                        principalTable: "SchoolInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "studentsReportsKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Keys = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SchoolInfoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_studentsReportsKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_studentsReportsKeys_SchoolInfos_SchoolInfoId",
                        column: x => x.SchoolInfoId,
                        principalTable: "SchoolInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "contractPages",
                columns: new[] { "Id", "ConPage", "SchoolInfoId" },
                values: new object[,]
                {
                    { 1, "<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'><meta name='viewport' content='width=device-width, initial-scale=1.0'><title>page 1</title></head><body> <style> .container { width: 100%; margin: 0 auto; padding: 10px; font-family: Arial, Helvetica, sans-serif; font-size: 16px; size: A4; } .container * { font-size: 14px; } h3 { font-size: 16px !important; } .containerLeft { text-align: left; padding: 10px 20px; } .containerRight { text-align: left; padding:10px 20px; } </style> <table style='width:100%;'> <tr> <td style='width:10%;text-align:right;padding: 0;'>  <img alt='logo' width='80px' style='padding:0;' src='data:image/png;base64,{fekrahublogo}'> </td> <td style='width:10%;text-align:left;padding: 0;'> <h1 style='font-size: 28px;'>FekraHubApp</h1> </td> <td style='width:80%;'></td> </tr> </table> <hr> <div class='container'> <table class='content'> <tr> <td class='containerLeft' style='vertical-align: top;width:50%;'> <h3 style='margin-bottom: 0;'>Kursvertrag für Arabischunterricht </h3> <span>zwischen Fekra, der beste Weg zum Arabisch Lernen,Sospanhie-Charlotten-Str. 90, 14059 Berlin vertreten durch denInhaber Yousef El Dada und den</span><b>Schüler / den</b> <b style='display: block;'>Schülerinnen:</b><br> <table style='width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;'> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Name</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{student.FirstName} {student.LastName}</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Geb.Datum</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'> {student.Birthday.Date.ToString('yyyy-MM-dd')}</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Geb.Ort</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{student.Nationality} </td> </tr> </table> <h5>gesetzlich vertreten durch die Erziehungsberechtigten:</h5> <table style='width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;'> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Name</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{parent.FirstName} {parent.LastName}</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Straße</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{parent.Street} {parent.StreetNr}</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>PLZ, Ort</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{parent.ZipCode}</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Tel. Mutter</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'> {parent.EmergencyPhoneNumber}</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Tel. Vater</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{parent.PhoneNumber} </td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Email</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>{parent.Email}</td> </tr> </table> <h5>1. Bildungsziele</h5><span>Die Schüler/Schülerinnen werden in arabischer Sprache alsFremdsprache/Herkunftssprache und in arabischerLandeskunde unterrichtet.</span> <h5>2. Unterrichtsort, Unterrichtstermine</h5><span>Der Unterricht wird in den jeweiligen Schulräumen vonFekra, der beste Weg zum Arabisch Lernen durchgeführt.Der Unterricht findet in der Regel einmal Wöchentlich (sa) inder Zeit von 10:00 bis 13:00 statt.</span> <h5>3. Aufsicht</h5><span>Die Hausordnung des Unterrichtsortes muss in der jeweilsgültigen Fassung eingehalten werden. Die Aufsichtspflichtder Übungsleiter/innen beschränkt sich auf den vermietetenUnterrichtsbereich der Schulräume sowie auf dieUnterrichtszeiten.</span> <h5>4. Haftung</h5><span>Für Schäden, die von Schülern/Schülerinnen verursachtwerden, haften diese oder die Eltern/Erziehungsberechtigtenim Rahmen der gesetzlichen Bestimmungen. Fekra, derbeste Weg zum Arabisch Lernen unterhält eineHaftpflichtversicherung nur für die Dauer der Erteilung desUnterrichts; sie ist auf Vorsatz und grobe Fahrlässigkeitbeschränkt. Den Eltern/Erziehungsberechtigten wirdempfohlen, für den/die Schüler/in - sofern nicht schongeschehen - eine private Haftpflichtversicherung undUnfallversicherung abzuschließen. Für den Verlustmitgebrachter Sachen (z.B. Fahrrad, Handy, Schmuck,Garderobe) wird keine Haftung übernommen.</span> </td> <td class='containerRight' style='vertical-align: top;width:50%;'> <h5>5. Kursgebühren (Zzgl. Unterrichtsmaterial)</h5><span>Es wird eine einmalige Anmeldegebühr von 0.00 EUR proSchüler erhoben.</span><br><span>Die Kursgebühren betragen für ein Schuljahr 750.00 EURpro Schüler ( Brutto).</span><br></br><span><u>Die Nichtteilnahme eines Schülers/Schülerin am Unterricht befreit ihn/sie nicht von der Entrichtung der Kursgebühren.</u>. In besonderen Fällen, wie dauerhafte Erkrankung, Ortswechsel und dergleichen, verzichtet Fekra, der beste Weg zum Arabisch Lernen auf die Kursgebühren. Die besonderen Gründe sind jedoch zu belegen.</span><br></br><span>Die Überweisung der Kursgebühren für das Schuljahr in Höhe von 750.00 EUR Brutto pro Schüler wird unter Angabe der Rechnungsnummer zum jeweils 1. eines jeden Monats auf folgendes Bankkonto erbeten:</span><br></br> <table style='width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;'> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Empfänger</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Yousef El Dada</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>IBAN</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>DE09 1005 0000 0190 7682 40</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>BIC</td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>BELADEBEXXX</td> </tr> <tr> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>Verwendungszweck </td> <td style=' border: 1px solid #000; padding: 5px; text-align: left;'>k Rechnungsnummer</td> </tr> </table> <h5>6. Zeugnis</h5><span>Die erbrachten Leistungen des Schülers/der Schülerin werden am Ende des Schuljahres in schriftlicher Form bestätigt. Die Aushändigung des Zeugnisses ist an die vollständige Begleichung der jährlichen Kursgebühr gebunden.</span> <h5>7. Dauer, Beendigung des Schulvertrages</h5><span>Der Schulvertrag wird auf unbestimmte Zeit abgeschlossen. Er endet:</span><br></br><span>a- wenn Fekra, der beste Weg zum Arabisch Lernen die Geschäftstätigkeit einstellt.</span></br><span>Oder:</span><br></br><span>b- durch Kündigung zum Ende des Schuljahres (bis 30.06.2022).</span><br></br><span><u>Die Kündigung erfolgt nur in schriftlicher Form</u></span><br></br><span>Unterbleibt eine Kündigung, verlängert sich der Schulvertrag stillschweigend um ein weiteres Schuljahr und kann erst zum nächstmöglichen Termin gekündigt werden.</span> </td> </tr> </table> <table style='width:100%;margin-top:15px'> <tr> <td style='width:10%;text-align:right;padding: 0;'> <div style='text-align:left;'> <h4>Telefon : 01794169927<br></br>Email : Admin@fekraschule.de<br></br>Adresse : Kleiststraße 23-26, 10787 Berlin</h4> </div> </td> <td style='width:10%;text-align:left;padding: 0;'> <div dir='rtl' style='text-align: right;'> <div style='color: white;background-color: black;width: 80%;margin:0 auto ;'> <h1 style='padding: 20px;font-size: 18px;'>مدرسة فكرة ... <br>الطريق الأفضل لتعلم اللغة العربية</h1> </div> </div> </td> </tr> </table> </div> </body> </html>", 1 },
                    { 2, "<!DOCTYPE html> <html lang='en'> <head> <meta charset='UTF-8'> <meta name='viewport' content='width=device-width, initial-scale=1.0'> <title>page 2</title> </head> <body> <style>  .container { width: 100%; margin: 0 auto; padding: 10px; font-family: Arial, Helvetica, sans-serif; font-size: 16px; size: A4; } .container * { font-size: 14px; } h3 { font-size: 16px !important; } .containerLeft { text-align: left; padding: 10px 20px; } .containerRight { text-align: left; padding:10px 20px; } </style> <table style='width:100%;'> <tr > <td style='width:10%;text-align:right;padding: 0;'> <img alt='logo' width='80px' style='padding:0;'src='data:image/png;base64,{fekrahublogo}' > </td> <td style='width:10%;text-align:left;padding: 0;'> <h1 style='font-size: 28px;'>FekraHubApp</h1> </td> <td style='width:80%;'></td> </tr> </table> <hr> <div class='container'>  <table class='content'> <tr> <td class='containerLeft' style='vertical-align: top;'> <h1>Information zur Verarbeitung<br>personenbezogener Daten</h1> <span>Verantwortlicher:</span><br></br><span>Yousef El Dada Sophie-Charlotten- Str. 90 14059 Berlin Email: Fekraberlin@gmail.com Telefon: +49 176 43210167</span><br></br><span> 1.Erhebung und Speicherung personenbezogener Daten, Art und Zweck und deren Verwendung Wenn Sie sich als Interessent*in und / oder Teilnehmer*in unserer Kurse registrieren: </span><span>• Anrede, Vorname, Nachname (bei Kindern: des/der Erziehungsberechtigten)</span><br><span>• Anschrift,</span><br><span>• Telefonnummer (Festnetz und/oder Mobilfunk), </span><br><span>• gültige E-Mail-Adresse,</span><br><span>• Geburtsdatum</span><br><span>• ggf. Bankverbindungsdaten (bei Lastschrifteinzugsverfahren)</span><br><br></br><span> Diese Daten sind notwendig zu eventuellen Vertragsabschlüssen und Korrespondenz mit Ihnen, zur Rechnungsstellung, zur Abwicklung von evtl. vorliegenden Haftungsansprüchen oder der Geltendmachung etwaiger Ansprüche gegen Sie. Die Datenverarbeitung erfolgt nach Art. 6 Abs. 1 S. 1 lit. b DSGVO zu den genannten und ist für die beidseitige Erfüllung von Verpflichtungen aus dem jeweiligen Vertrags erforderlich. Die von uns erhobenen personenbezogenen Daten werden bis zum Ablauf der gesetzlichen Aufbewahrungs- und Dokumentationspflichten (aus HGB, StGB oder AO) nach Art. 6 Abs. 1 S.1 lit. C DSGVO gelöscht, es sei denn, dass Sie in eine darüber hinausgehende Speicherung nach Art. 6 Abs. 1 S.1 lit. C DSGVO eingewilligt haben. </span><br></br><span>2. Weitergabe von Daten an Dritte </span><br></br><span>Eine Übermittlung Ihrer persönlichen Daten an Dritte zu anderen als den im Folgenden aufgeführten Zwecken findet nicht statt. Nur soweit dies nach Art. 6 Abs. 1 S. 1 lit. b DSGVO für die Abwicklung unseres Auftrags mit Ihnen erforderlich ist, werden Ihre personenbezogenen Daten an Dritte weitergegeben. Hierzu gehört insbesondere die Weitergabe an unsere Mitarbeiter, die soweit für die Aufrechterhaltung Abläufe notwendig ist, unsere Buchhaltung, steuerliche und Unternehmens-berater sowie auf Verlangen an öffentliche Behörden, wie das Finanzamt. </span><br></br><span>3. Einwilligung in die Datennutzung zu Werbe- und PR- Zwecken Sind Sie mit den folgenden Nutzungszwecken einverstanden, kreuzen Sie diese bitte entsprechend an. Wollen Sie keine Einwilligung erteilen, lassen Sie das Feld bitte frei. </span><br></br><span>▢ Ich willige ein, dass wir im Rahmen unsere schulischen Arbeit Fotound Videoaufnahmen in unseren Räumen von Ihnen bzw. Ihren Kindern machen, die für unsere Webauftritte, zu PR- und Werbezwecken nutzen. </span><br></br><span>4. Betroffenenrechte </span><br></br> <ul> <li>Sie haben das Recht: gemäß Art. 7 Abs. 3 DSGVO Ihre einmal erteilte Einwilligung jederzeit gegenüber uns zu widerrufen. Dies hat zur Folge, dass wir die Datenverarbeitung, die auf dieser Einwilligung beruhte, für die Zukunft nicht mehr fortführen dürfen; </li> </ul> </td> <td class='containerLeft' style='vertical-align: top;'> <ul> <li>gemäß Art. 15 DSGVO Auskunft über Ihre von uns verarbeiteten personenbezogenen Daten zu verlangen. Insbesondere können Sie Auskunft über die Verarbeitungszwecke, die Kategorie der personenbezogenen Daten, die Kategorien von Empfängern, gegenüber denen Ihre Daten offengelegt wurden oder werden, die geplante Speicherdauer, das Bestehen eines Rechts auf Berichtigung, Löschung, Einschränkung der Verarbeitung oder Widerspruch, das Bestehen eines Beschwerderechts, die Herkunft ihrer Daten, sofern diese nicht bei uns erhoben wurden, sowie über das Bestehen einer automatisierten Entscheidungsfindung einschließlich Profiling und ggf. aussagekräftigen Informationen zu deren Einzelheitenverlangen; </li> </ul><br></br><span>• gemäß Art. 16 DSGVO unverzüglich die Berichtigung unrichtiger oder Vervollständigung Ihrer bei uns gespeicherten personenbezogenen Daten zu verlangen; </span><br></br><span>• gemäß Art. 17 DSGVO die Löschung Ihrer bei uns gespeicherten personenbezogenen Daten zu verlangen, soweit nicht die Verarbeitung zur Ausübung des Rechts auf freie Meinungsäußerung und Information, zur Erfüllung einer rechtlichen Verpflichtung, aus Gründen des öffentlichen Interesses oder zur Geltendmachung, Ausübung oder Verteidigung von Rechtsansprüchen erforderlich ist; </span><br></br><span>• gemäß Art. 18 DSGVO die Einschränkung der Verarbeitung Ihrer personenbezogenen Daten zu verlangen, soweit die Richtigkeit der Daten von Ihnen bestritten wird, die Verarbeitung unrechtmäßig ist, Sie aber deren Löschung ablehnen und wir die Daten nicht mehr benötigen, Sie jedoch diese zur Geltendmachung, Ausübung oder Verteidigung von Rechtsansprüchen benötigen oder Sie gemäß Art. 21 DSGVO Widerspruch gegen die Verarbeitung eingelegt haben; </span><br></br><span>• gemäß Art. 20 DSGVO Ihre personenbezogenen Daten ,die Sie uns bereitgestellt haben, in einem strukturierten, gängigen und maschinenlesebaren Format zu erhalten oder die Übermittlung an einen anderen Verantwortlichen zu verlangen;</span><br></br><span>• gemäß Art. 77 DSGVO sich bei einer Aufsichtsbehörde zu beschweren. In der Regel können Sie sich hierfür an die Aufsichtsbehörde Ihres üblichen Aufenthaltsortes oder Arbeitsplatzes oder unseres Firmensitzes wenden</span><br></br><span><b> Widerspruchsrecht </b></span><br></br><span>Sofern Ihre personenbezogenen Daten auf Grundlage von berechtigten Interessen gemäß Art. 6 Abs. 1 S. 1 lit. f DSGVO verarbeitet werden, haben Sie das Recht, gemäß Art. 21 DSGVO Widerspruch gegen die Verarbeitung Ihrer personenbezogenen Daten einzulegen, soweit dafür Gründe vorliegen, die sich aus Ihrer besonderen Situation ergeben. Mochten Sie von Ihrem Widerspruchsrecht Gebrauch machen, genügt eine E-Mail an uns</span> </td> </tr> </table> <table style='width:100%;margin-top:15px'> <tr > <td style='width:10%;text-align:right;padding: 0;'> <div style='text-align:left;'> <h4>Telefon : 01794169927<br></br>Email : Admin@fekraschule.de<br></br>Adresse : Kleiststraße 23-26, 10787 Berlin</h4> </div> </td> <td style='width:10%;text-align:left;padding: 0;'> <div dir='rtl' style='text-align: right;'> <div style='color: white;background-color: black;width: 80%;margin:0 auto ;'> <h1 style='padding: 20px;font-size: 18px;'>مدرسة فكرة ... <br>الطريق الأفضل لتعلم اللغة العربية</h1> </div> </div> </td> </tr> </table> </div> </body> </html>", 1 }
                });

            migrationBuilder.InsertData(
                table: "studentsReportsKeys",
                columns: new[] { "Id", "Keys", "SchoolInfoId" },
                values: new object[,]
                {
                    { 1, "الإنتباه في الدرس", 1 },
                    { 2, "المشاركة في الصف", 1 },
                    { 3, "الإملاء", 1 },
                    { 4, "القراءة", 1 },
                    { 5, "الخط", 1 },
                    { 6, "القواعد", 1 },
                    { 7, "إتقان الدرس", 1 },
                    { 8, "روح المنافسة", 1 },
                    { 9, "الإندماج في الصف مع كل من المعلم والتلاميذ الآخرين", 1 },
                    { 10, "الإلتزام بأوقات الدوام", 1 },
                    { 11, "الإلتزام بقواعد الأدب العامة", 1 },
                    { 12, "ملاحظات إضافية", 1 },
                    { 13, "الإقتراحات", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_contractPages_SchoolInfoId",
                table: "contractPages",
                column: "SchoolInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_studentsReportsKeys_SchoolInfoId",
                table: "studentsReportsKeys",
                column: "SchoolInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contractPages");

            migrationBuilder.DropTable(
                name: "studentsReportsKeys");

            migrationBuilder.AlterColumn<string>(
                name: "UrlDomain",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolOwner",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SchoolName",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PrivacyPolicy",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Password",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LogoBase64",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromEmail",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmailServer",
                table: "SchoolInfos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractPages",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StudentsReportsKeys",
                table: "SchoolInfos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "SchoolInfos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ContractPages", "StudentsReportsKeys" },
                values: new object[] { "[\"\\u003C!DOCTYPE html\\u003E\\u003Chtml lang=\\u0027en\\u0027\\u003E\\u003Chead\\u003E\\u003Cmeta charset=\\u0027UTF-8\\u0027\\u003E\\u003Cmeta name=\\u0027viewport\\u0027 content=\\u0027width=device-width, initial-scale=1.0\\u0027\\u003E\\u003Ctitle\\u003Epage 1\\u003C/title\\u003E\\u003C/head\\u003E\\u003Cbody\\u003E \\u003Cstyle\\u003E .container { width: 100%; margin: 0 auto; padding: 10px; font-family: Arial, Helvetica, sans-serif; font-size: 16px; size: A4; } .container * { font-size: 14px; } h3 { font-size: 16px !important; } .containerLeft { text-align: left; padding: 10px 20px; } .containerRight { text-align: left; padding:10px 20px; } \\u003C/style\\u003E \\u003Ctable style=\\u0027width:100%;\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027width:10%;text-align:right;padding: 0;\\u0027\\u003E  \\u003Cimg alt=\\u0027logo\\u0027 width=\\u002780px\\u0027 style=\\u0027padding:0;\\u0027 src=\\u0027data:image/png;base64,{fekrahublogo}\\u0027\\u003E \\u003C/td\\u003E \\u003Ctd style=\\u0027width:10%;text-align:left;padding: 0;\\u0027\\u003E \\u003Ch1 style=\\u0027font-size: 28px;\\u0027\\u003EFekraHubApp\\u003C/h1\\u003E \\u003C/td\\u003E \\u003Ctd style=\\u0027width:80%;\\u0027\\u003E\\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Chr\\u003E \\u003Cdiv class=\\u0027container\\u0027\\u003E \\u003Ctable class=\\u0027content\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd class=\\u0027containerLeft\\u0027 style=\\u0027vertical-align: top;width:50%;\\u0027\\u003E \\u003Ch3 style=\\u0027margin-bottom: 0;\\u0027\\u003EKursvertrag f\\u00FCr Arabischunterricht \\u003C/h3\\u003E \\u003Cspan\\u003Ezwischen Fekra, der beste Weg zum Arabisch Lernen,Sospanhie-Charlotten-Str. 90, 14059 Berlin vertreten durch denInhaber Yousef El Dada und den\\u003C/span\\u003E\\u003Cb\\u003ESch\\u00FCler / den\\u003C/b\\u003E \\u003Cb style=\\u0027display: block;\\u0027\\u003ESch\\u00FClerinnen:\\u003C/b\\u003E\\u003Cbr\\u003E \\u003Ctable style=\\u0027width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EName\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{student.FirstName} {student.LastName}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EGeb.Datum\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E {student.Birthday.Date.ToString(\\u0027yyyy-MM-dd\\u0027)}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EGeb.Ort\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{student.Nationality} \\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Ch5\\u003Egesetzlich vertreten durch die Erziehungsberechtigten:\\u003C/h5\\u003E \\u003Ctable style=\\u0027width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EName\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{parent.FirstName} {parent.LastName}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EStra\\u00DFe\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{parent.Street} {parent.StreetNr}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EPLZ, Ort\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{parent.ZipCode}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003ETel. Mutter\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E {parent.EmergencyPhoneNumber}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003ETel. Vater\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{parent.PhoneNumber} \\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EEmail\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003E{parent.Email}\\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Ch5\\u003E1. Bildungsziele\\u003C/h5\\u003E\\u003Cspan\\u003EDie Sch\\u00FCler/Sch\\u00FClerinnen werden in arabischer Sprache alsFremdsprache/Herkunftssprache und in arabischerLandeskunde unterrichtet.\\u003C/span\\u003E \\u003Ch5\\u003E2. Unterrichtsort, Unterrichtstermine\\u003C/h5\\u003E\\u003Cspan\\u003EDer Unterricht wird in den jeweiligen Schulr\\u00E4umen vonFekra, der beste Weg zum Arabisch Lernen durchgef\\u00FChrt.Der Unterricht findet in der Regel einmal W\\u00F6chentlich (sa) inder Zeit von 10:00 bis 13:00 statt.\\u003C/span\\u003E \\u003Ch5\\u003E3. Aufsicht\\u003C/h5\\u003E\\u003Cspan\\u003EDie Hausordnung des Unterrichtsortes muss in der jeweilsg\\u00FCltigen Fassung eingehalten werden. Die Aufsichtspflichtder \\u00DCbungsleiter/innen beschr\\u00E4nkt sich auf den vermietetenUnterrichtsbereich der Schulr\\u00E4ume sowie auf dieUnterrichtszeiten.\\u003C/span\\u003E \\u003Ch5\\u003E4. Haftung\\u003C/h5\\u003E\\u003Cspan\\u003EF\\u00FCr Sch\\u00E4den, die von Sch\\u00FClern/Sch\\u00FClerinnen verursachtwerden, haften diese oder die Eltern/Erziehungsberechtigtenim Rahmen der gesetzlichen Bestimmungen. Fekra, derbeste Weg zum Arabisch Lernen unterh\\u00E4lt eineHaftpflichtversicherung nur f\\u00FCr die Dauer der Erteilung desUnterrichts; sie ist auf Vorsatz und grobe Fahrl\\u00E4ssigkeitbeschr\\u00E4nkt. Den Eltern/Erziehungsberechtigten wirdempfohlen, f\\u00FCr den/die Sch\\u00FCler/in - sofern nicht schongeschehen - eine private Haftpflichtversicherung undUnfallversicherung abzuschlie\\u00DFen. F\\u00FCr den Verlustmitgebrachter Sachen (z.B. Fahrrad, Handy, Schmuck,Garderobe) wird keine Haftung \\u00FCbernommen.\\u003C/span\\u003E \\u003C/td\\u003E \\u003Ctd class=\\u0027containerRight\\u0027 style=\\u0027vertical-align: top;width:50%;\\u0027\\u003E \\u003Ch5\\u003E5. Kursgeb\\u00FChren (Zzgl. Unterrichtsmaterial)\\u003C/h5\\u003E\\u003Cspan\\u003EEs wird eine einmalige Anmeldegeb\\u00FChr von 0.00 EUR proSch\\u00FCler erhoben.\\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cspan\\u003EDie Kursgeb\\u00FChren betragen f\\u00FCr ein Schuljahr 750.00 EURpro Sch\\u00FCler ( Brutto).\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u003Cu\\u003EDie Nichtteilnahme eines Sch\\u00FClers/Sch\\u00FClerin am Unterricht befreit ihn/sie nicht von der Entrichtung der Kursgeb\\u00FChren.\\u003C/u\\u003E. In besonderen F\\u00E4llen, wie dauerhafte Erkrankung, Ortswechsel und dergleichen, verzichtet Fekra, der beste Weg zum Arabisch Lernen auf die Kursgeb\\u00FChren. Die besonderen Gr\\u00FCnde sind jedoch zu belegen.\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003EDie \\u00DCberweisung der Kursgeb\\u00FChren f\\u00FCr das Schuljahr in H\\u00F6he von 750.00 EUR Brutto pro Sch\\u00FCler wird unter Angabe der Rechnungsnummer zum jeweils 1. eines jeden Monats auf folgendes Bankkonto erbeten:\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E \\u003Ctable style=\\u0027width: 100%; border-collapse: collapse; border: 1px solid #ddd;text-align: center;\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EEmpf\\u00E4nger\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EYousef El Dada\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EIBAN\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EDE09 1005 0000 0190 7682 40\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EBIC\\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EBELADEBEXXX\\u003C/td\\u003E \\u003C/tr\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003EVerwendungszweck \\u003C/td\\u003E \\u003Ctd style=\\u0027 border: 1px solid #000; padding: 5px; text-align: left;\\u0027\\u003Ek Rechnungsnummer\\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Ch5\\u003E6. Zeugnis\\u003C/h5\\u003E\\u003Cspan\\u003EDie erbrachten Leistungen des Sch\\u00FClers/der Sch\\u00FClerin werden am Ende des Schuljahres in schriftlicher Form best\\u00E4tigt. Die Aush\\u00E4ndigung des Zeugnisses ist an die vollst\\u00E4ndige Begleichung der j\\u00E4hrlichen Kursgeb\\u00FChr gebunden.\\u003C/span\\u003E \\u003Ch5\\u003E7. Dauer, Beendigung des Schulvertrages\\u003C/h5\\u003E\\u003Cspan\\u003EDer Schulvertrag wird auf unbestimmte Zeit abgeschlossen. Er endet:\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003Ea- wenn Fekra, der beste Weg zum Arabisch Lernen die Gesch\\u00E4ftst\\u00E4tigkeit einstellt.\\u003C/span\\u003E\\u003C/br\\u003E\\u003Cspan\\u003EOder:\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003Eb- durch K\\u00FCndigung zum Ende des Schuljahres (bis 30.06.2022).\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u003Cu\\u003EDie K\\u00FCndigung erfolgt nur in schriftlicher Form\\u003C/u\\u003E\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003EUnterbleibt eine K\\u00FCndigung, verl\\u00E4ngert sich der Schulvertrag stillschweigend um ein weiteres Schuljahr und kann erst zum n\\u00E4chstm\\u00F6glichen Termin gek\\u00FCndigt werden.\\u003C/span\\u003E \\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Ctable style=\\u0027width:100%;margin-top:15px\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd style=\\u0027width:10%;text-align:right;padding: 0;\\u0027\\u003E \\u003Cdiv style=\\u0027text-align:left;\\u0027\\u003E \\u003Ch4\\u003ETelefon : 01794169927\\u003Cbr\\u003E\\u003C/br\\u003EEmail : Admin@fekraschule.de\\u003Cbr\\u003E\\u003C/br\\u003EAdresse : Kleiststra\\u00DFe 23-26,\\u00A010787\\u00A0Berlin\\u003C/h4\\u003E \\u003C/div\\u003E \\u003C/td\\u003E \\u003Ctd style=\\u0027width:10%;text-align:left;padding: 0;\\u0027\\u003E \\u003Cdiv dir=\\u0027rtl\\u0027 style=\\u0027text-align: right;\\u0027\\u003E \\u003Cdiv style=\\u0027color: white;background-color: black;width: 80%;margin:0 auto ;\\u0027\\u003E \\u003Ch1 style=\\u0027padding: 20px;font-size: 18px;\\u0027\\u003E\\u0645\\u062F\\u0631\\u0633\\u0629 \\u0641\\u0643\\u0631\\u0629 ... \\u003Cbr\\u003E\\u0627\\u0644\\u0637\\u0631\\u064A\\u0642 \\u0627\\u0644\\u0623\\u0641\\u0636\\u0644 \\u0644\\u062A\\u0639\\u0644\\u0645 \\u0627\\u0644\\u0644\\u063A\\u0629 \\u0627\\u0644\\u0639\\u0631\\u0628\\u064A\\u0629\\u003C/h1\\u003E \\u003C/div\\u003E \\u003C/div\\u003E \\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003C/div\\u003E \\u003C/body\\u003E \\u003C/html\\u003E\",\"\\u003C!DOCTYPE html\\u003E \\u003Chtml lang=\\u0027en\\u0027\\u003E \\u003Chead\\u003E \\u003Cmeta charset=\\u0027UTF-8\\u0027\\u003E \\u003Cmeta name=\\u0027viewport\\u0027 content=\\u0027width=device-width, initial-scale=1.0\\u0027\\u003E \\u003Ctitle\\u003Epage 2\\u003C/title\\u003E \\u003C/head\\u003E \\u003Cbody\\u003E \\u003Cstyle\\u003E  .container { width: 100%; margin: 0 auto; padding: 10px; font-family: Arial, Helvetica, sans-serif; font-size: 16px; size: A4; } .container * { font-size: 14px; } h3 { font-size: 16px !important; } .containerLeft { text-align: left; padding: 10px 20px; } .containerRight { text-align: left; padding:10px 20px; } \\u003C/style\\u003E \\u003Ctable style=\\u0027width:100%;\\u0027\\u003E \\u003Ctr \\u003E \\u003Ctd style=\\u0027width:10%;text-align:right;padding: 0;\\u0027\\u003E \\u003Cimg alt=\\u0027logo\\u0027 width=\\u002780px\\u0027 style=\\u0027padding:0;\\u0027src=\\u0027data:image/png;base64,{fekrahublogo}\\u0027 \\u003E \\u003C/td\\u003E \\u003Ctd style=\\u0027width:10%;text-align:left;padding: 0;\\u0027\\u003E \\u003Ch1 style=\\u0027font-size: 28px;\\u0027\\u003EFekraHubApp\\u003C/h1\\u003E \\u003C/td\\u003E \\u003Ctd style=\\u0027width:80%;\\u0027\\u003E\\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Chr\\u003E \\u003Cdiv class=\\u0027container\\u0027\\u003E  \\u003Ctable class=\\u0027content\\u0027\\u003E \\u003Ctr\\u003E \\u003Ctd class=\\u0027containerLeft\\u0027 style=\\u0027vertical-align: top;\\u0027\\u003E \\u003Ch1\\u003EInformation zur Verarbeitung\\u003Cbr\\u003Epersonenbezogener Daten\\u003C/h1\\u003E \\u003Cspan\\u003EVerantwortlicher:\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003EYousef El Dada Sophie-Charlotten- Str. 90 14059 Berlin Email: Fekraberlin@gmail.com Telefon: \\u002B49 176 43210167\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E 1.Erhebung und Speicherung personenbezogener Daten, Art und Zweck und deren Verwendung Wenn Sie sich als Interessent*in und / oder Teilnehmer*in unserer Kurse registrieren: \\u003C/span\\u003E\\u003Cspan\\u003E\\u2022 Anrede, Vorname, Nachname (bei Kindern: des/der Erziehungsberechtigten)\\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cspan\\u003E\\u2022 Anschrift,\\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cspan\\u003E\\u2022 Telefonnummer (Festnetz und/oder Mobilfunk), \\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cspan\\u003E\\u2022 gu\\u0308ltige E-Mail-Adresse,\\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cspan\\u003E\\u2022 Geburtsdatum\\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cspan\\u003E\\u2022 ggf. Bankverbindungsdaten (bei Lastschrifteinzugsverfahren)\\u003C/span\\u003E\\u003Cbr\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E Diese Daten sind notwendig zu eventuellen Vertragsabschlu\\u0308ssen und Korrespondenz mit Ihnen, zur Rechnungsstellung, zur Abwicklung von evtl. vorliegenden Haftungsanspru\\u0308chen oder der Geltendmachung etwaiger Anspru\\u0308che gegen Sie. Die Datenverarbeitung erfolgt nach Art. 6 Abs. 1 S. 1 lit. b DSGVO zu den genannten und ist fu\\u0308r die beidseitige Erfu\\u0308llung von Verpflichtungen aus dem jeweiligen Vertrags erforderlich. Die von uns erhobenen personenbezogenen Daten werden bis zum Ablauf der gesetzlichen Aufbewahrungs- und Dokumentationspflichten (aus HGB, StGB oder AO) nach Art. 6 Abs. 1 S.1 lit. C DSGVO gelo\\u0308scht, es sei denn, dass Sie in eine daru\\u0308ber hinausgehende Speicherung nach Art. 6 Abs. 1 S.1 lit. C DSGVO eingewilligt haben. \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E2. Weitergabe von Daten an Dritte \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003EEine U\\u0308bermittlung Ihrer perso\\u0308nlichen Daten an Dritte zu anderen als den im Folgenden aufgefu\\u0308hrten Zwecken findet nicht statt. Nur soweit dies nach Art. 6 Abs. 1 S. 1 lit. b DSGVO fu\\u0308r die Abwicklung unseres Auftrags mit Ihnen erforderlich ist, werden Ihre personenbezogenen Daten an Dritte weitergegeben. Hierzu geho\\u0308rt insbesondere die Weitergabe an unsere Mitarbeiter, die soweit fu\\u0308r die Aufrechterhaltung Abla\\u0308ufe notwendig ist, unsere Buchhaltung, steuerliche und Unternehmens-berater sowie auf Verlangen an o\\u0308ffentliche Beho\\u0308rden, wie das Finanzamt. \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E3. Einwilligung in die Datennutzung zu Werbe- und PR- Zwecken Sind Sie mit den folgenden Nutzungszwecken einverstanden, kreuzen Sie diese bitte entsprechend an. Wollen Sie keine Einwilligung erteilen, lassen Sie das Feld bitte frei. \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u25A2 Ich willige ein, dass wir im Rahmen unsere schulischen Arbeit Fotound Videoaufnahmen in unseren Ra\\u0308umen von Ihnen bzw. Ihren Kindern machen, die fu\\u0308r unsere Webauftritte, zu PR- und Werbezwecken nutzen. \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E4. Betroffenenrechte \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E \\u003Cul\\u003E \\u003Cli\\u003ESie haben das Recht: gema\\u0308\\u00DF Art. 7 Abs. 3 DSGVO Ihre einmal erteilte Einwilligung jederzeit gegenu\\u0308ber uns zu widerrufen. Dies hat zur Folge, dass wir die Datenverarbeitung, die auf dieser Einwilligung beruhte, fu\\u0308r die Zukunft nicht mehr fortfu\\u0308hren du\\u0308rfen; \\u003C/li\\u003E \\u003C/ul\\u003E \\u003C/td\\u003E \\u003Ctd class=\\u0027containerLeft\\u0027 style=\\u0027vertical-align: top;\\u0027\\u003E \\u003Cul\\u003E \\u003Cli\\u003Egema\\u0308\\u00DF Art. 15 DSGVO Auskunft u\\u0308ber Ihre von uns verarbeiteten personenbezogenen Daten zu verlangen. Insbesondere ko\\u0308nnen Sie Auskunft u\\u0308ber die Verarbeitungszwecke, die Kategorie der personenbezogenen Daten, die Kategorien von Empfa\\u0308ngern, gegenu\\u0308ber denen Ihre Daten offengelegt wurden oder werden, die geplante Speicherdauer, das Bestehen eines Rechts auf Berichtigung, Lo\\u0308schung, Einschr\\u00E4nkung der Verarbeitung oder Widerspruch, das Bestehen eines Beschwerderechts, die Herkunft ihrer Daten, sofern diese nicht bei uns erhoben wurden, sowie u\\u0308ber das Bestehen einer automatisierten Entscheidungsfindung einschlie\\u00DFlich Profiling und ggf. aussagekr\\u00E4ftigen Informationen zu deren Einzelheitenverlangen; \\u003C/li\\u003E \\u003C/ul\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u2022 gem\\u00E4\\u00DF Art. 16 DSGVO unverz\\u00FCglich die Berichtigung unrichtiger oder Vervollst\\u00E4ndigung Ihrer bei uns gespeicherten personenbezogenen Daten zu verlangen; \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u2022 gem\\u00E4\\u00DF Art. 17 DSGVO die L\\u00F6schung Ihrer bei uns gespeicherten personenbezogenen Daten zu verlangen, soweit nicht die Verarbeitung zur Aus\\u00FCbung des Rechts auf freie Meinungs\\u00E4u\\u00DFerung und Information, zur Erf\\u00FCllung einer rechtlichen Verpflichtung, aus Gr\\u00FCnden des \\u00F6ffentlichen Interesses oder zur Geltendmachung, Aus\\u00FCbung oder Verteidigung von Rechtsanspru\\u0308chen erforderlich ist; \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u2022 gem\\u00E4\\u00DF Art. 18 DSGVO die Einschr\\u00E4nkung der Verarbeitung Ihrer personenbezogenen Daten zu verlangen, soweit die Richtigkeit der Daten von Ihnen bestritten wird, die Verarbeitung unrechtm\\u00E4\\u00DFig ist, Sie aber deren L\\u00F6schung ablehnen und wir die Daten nicht mehr ben\\u00F6tigen, Sie jedoch diese zur Geltendmachung, Aus\\u00FCbung oder Verteidigung von Rechtsanspr\\u00FCchen ben\\u00F6tigen oder Sie gem\\u00E4\\u00DF Art. 21 DSGVO Widerspruch gegen die Verarbeitung eingelegt haben; \\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u2022 gem\\u00E4\\u00DF Art. 20 DSGVO Ihre personenbezogenen Daten ,die Sie uns bereitgestellt haben, in einem strukturierten, g\\u00E4ngigen und maschinenlesebaren Format zu erhalten oder die \\u00DCbermittlung an einen anderen Verantwortlichen zu verlangen;\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u2022 gem\\u00E4\\u00DF Art. 77 DSGVO sich bei einer Aufsichtsbeh\\u00F6rde zu beschweren. In der Regel k\\u00F6nnen Sie sich hierf\\u00FCr an die Aufsichtsbeh\\u00F6rde Ihres \\u00FCblichen Aufenthaltsortes oder Arbeitsplatzes oder unseres Firmensitzes wenden\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003E\\u003Cb\\u003E Widerspruchsrecht \\u003C/b\\u003E\\u003C/span\\u003E\\u003Cbr\\u003E\\u003C/br\\u003E\\u003Cspan\\u003ESofern Ihre personenbezogenen Daten auf Grundlage von berechtigten Interessen gem\\u00E4\\u00DF Art. 6 Abs. 1 S. 1 lit. f DSGVO verarbeitet werden, haben Sie das Recht, gem\\u00E4\\u00DF Art. 21 DSGVO Widerspruch gegen die Verarbeitung Ihrer personenbezogenen Daten einzulegen, soweit daf\\u00FCr Gr\\u00FCnde vorliegen, die sich aus Ihrer besonderen Situation ergeben. Mochten Sie von Ihrem Widerspruchsrecht Gebrauch machen, gen\\u00FCgt eine E-Mail an uns\\u003C/span\\u003E \\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003Ctable style=\\u0027width:100%;margin-top:15px\\u0027\\u003E \\u003Ctr \\u003E \\u003Ctd style=\\u0027width:10%;text-align:right;padding: 0;\\u0027\\u003E \\u003Cdiv style=\\u0027text-align:left;\\u0027\\u003E \\u003Ch4\\u003ETelefon : 01794169927\\u003Cbr\\u003E\\u003C/br\\u003EEmail : Admin@fekraschule.de\\u003Cbr\\u003E\\u003C/br\\u003EAdresse : Kleiststra\\u00DFe 23-26,\\u00A010787\\u00A0Berlin\\u003C/h4\\u003E \\u003C/div\\u003E \\u003C/td\\u003E \\u003Ctd style=\\u0027width:10%;text-align:left;padding: 0;\\u0027\\u003E \\u003Cdiv dir=\\u0027rtl\\u0027 style=\\u0027text-align: right;\\u0027\\u003E \\u003Cdiv style=\\u0027color: white;background-color: black;width: 80%;margin:0 auto ;\\u0027\\u003E \\u003Ch1 style=\\u0027padding: 20px;font-size: 18px;\\u0027\\u003E\\u0645\\u062F\\u0631\\u0633\\u0629 \\u0641\\u0643\\u0631\\u0629 ... \\u003Cbr\\u003E\\u0627\\u0644\\u0637\\u0631\\u064A\\u0642 \\u0627\\u0644\\u0623\\u0641\\u0636\\u0644 \\u0644\\u062A\\u0639\\u0644\\u0645 \\u0627\\u0644\\u0644\\u063A\\u0629 \\u0627\\u0644\\u0639\\u0631\\u0628\\u064A\\u0629\\u003C/h1\\u003E \\u003C/div\\u003E \\u003C/div\\u003E \\u003C/td\\u003E \\u003C/tr\\u003E \\u003C/table\\u003E \\u003C/div\\u003E \\u003C/body\\u003E \\u003C/html\\u003E\"]", "[\"\\u0627\\u0644\\u0625\\u0646\\u062A\\u0628\\u0627\\u0647 \\u0641\\u064A \\u0627\\u0644\\u062F\\u0631\\u0633\",\"\\u0627\\u0644\\u0645\\u0634\\u0627\\u0631\\u0643\\u0629 \\u0641\\u064A \\u0627\\u0644\\u0635\\u0641\",\"\\u0627\\u0644\\u0625\\u0645\\u0644\\u0627\\u0621\",\"\\u0627\\u0644\\u0642\\u0631\\u0627\\u0621\\u0629\",\"\\u0627\\u0644\\u062E\\u0637\",\"\\u0627\\u0644\\u0642\\u0648\\u0627\\u0639\\u062F\",\"\\u0625\\u062A\\u0642\\u0627\\u0646 \\u0627\\u0644\\u062F\\u0631\\u0633\",\"\\u0631\\u0648\\u062D \\u0627\\u0644\\u0645\\u0646\\u0627\\u0641\\u0633\\u0629\",\"\\u0627\\u0644\\u0625\\u0646\\u062F\\u0645\\u0627\\u062C \\u0641\\u064A \\u0627\\u0644\\u0635\\u0641 \\u0645\\u0639 \\u0643\\u0644 \\u0645\\u0646 \\u0627\\u0644\\u0645\\u0639\\u0644\\u0645 \\u0648\\u0627\\u0644\\u062A\\u0644\\u0627\\u0645\\u064A\\u0630 \\u0627\\u0644\\u0622\\u062E\\u0631\\u064A\\u0646\",\"\\u0627\\u0644\\u0625\\u0644\\u062A\\u0632\\u0627\\u0645 \\u0628\\u0623\\u0648\\u0642\\u0627\\u062A \\u0627\\u0644\\u062F\\u0648\\u0627\\u0645\",\"\\u0627\\u0644\\u0625\\u0644\\u062A\\u0632\\u0627\\u0645 \\u0628\\u0642\\u0648\\u0627\\u0639\\u062F \\u0627\\u0644\\u0623\\u062F\\u0628 \\u0627\\u0644\\u0639\\u0627\\u0645\\u0629\",\"\\u0645\\u0644\\u0627\\u062D\\u0638\\u0627\\u062A \\u0625\\u0636\\u0627\\u0641\\u064A\\u0629\",\"\\u0627\\u0644\\u0625\\u0642\\u062A\\u0631\\u0627\\u062D\\u0627\\u062A\"]" });
        }
    }
}
