using Microsoft.EntityFrameworkCore;
using TenderquickServer.Models;
using TenderquickServer.Models.Tenders;

namespace TenderquickServer.Data
{
    // Applies pending migrations, then fills empty tables with a realistic Singapore M&E
    // dataset so the app is demonstrable the moment it boots. Each block is guarded by an
    // "is this table empty" check, so it never fights with real data.
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            await SeedUsersAsync(db);
            await SeedTendersAsync(db);
            await SeedInventoryAsync(db);
            await SeedLabourAsync(db);
            await SeedDeadlinesAsync(db);
            await SeedTemplatesAsync(db);
            await SeedQuotationsAsync(db);
            await SeedDraftsAsync(db);
            await SeedMemoryAsync(db);
        }

        private static DateTime At(int daysFromNow, int hour = 17, int minute = 0)
        {
            var day = DateTime.UtcNow.Date.AddDays(daysFromNow);
            return day.AddHours(hour).AddMinutes(minute);
        }

        private static async Task SeedUsersAsync(AppDbContext db)
        {
            if (await db.Users.AnyAsync()) return;

            db.Users.AddRange(
                NewUser("Admin User", "admin@tenderquick.local", "Admin#123", Roles.Admin),
                NewUser("Est User", "estimator@tenderquick.local", "Estimator#123", Roles.Estimator),
                NewUser("View User", "viewer@tenderquick.local", "Viewer#123", Roles.Viewer));

            await db.SaveChangesAsync();
        }

        private static User NewUser(string name, string email, string password, string role) => new()
        {
            Name = name,
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            CreatedAt = DateTime.UtcNow,
        };

        private static async Task SeedTendersAsync(AppDbContext db)
        {
            if (await db.Tenders.AnyAsync()) return;

            db.Tenders.AddRange(
                NewTender("GeBiz/ITQ/2026/0418",
                    "Supply & Installation of LED Lighting at HDB Blocks (Tampines)",
                    "Housing & Development Board", "GeBIZ", TenderStatus.Drafting, 185000m, At(5),
                    "Retrofit of common-area lighting across 14 blocks. T8 LED, motion sensors at staircases.",
                    new[]
                    {
                        "Supply and install 1,200 nos. LED batten 18W with motion sensor at staircases",
                        "Replace existing fluorescent fittings at common corridors",
                        "All works to comply with SS 531 and BCA Green Mark requirements",
                        "Contractor to hold valid bizSAFE Level 3 and EMA Class licence",
                        "12-month defects liability period",
                    }),
                NewTender("GeBiz/ITT/2026/0377",
                    "CCTV System Upgrade at Community Clubs (Central Region)",
                    "People's Association", "GeBIZ", TenderStatus.Interested, 320000m, At(12),
                    "Upgrade analogue CCTV to 4MP IP across 8 CCs. NVR with 30-day retention.",
                    new[]
                    {
                        "Supply and install 96 nos. 4MP IP dome cameras across 8 community clubs",
                        "Network video recorders with minimum 30-day footage retention",
                        "Integration with existing PA security operations centre",
                        "Cat6 structured cabling and PoE switches",
                    }),
                NewTender("SESAMI/RFQ/2026/1182",
                    "Periodic M&E Maintenance for Polyclinics",
                    "National Healthcare Group", "Sesami", TenderStatus.Submitted, 540000m, At(-2),
                    "2-year M&E maintenance, quarterly servicing. Submitted, awaiting outcome.",
                    new[]
                    {
                        "Quarterly preventive maintenance of electrical, ACMV and plumbing systems",
                        "Provision of standby manpower for fault response within 4 hours",
                        "24/7 emergency call-out coverage",
                    }),
                NewTender("GeBiz/ITQ/2026/0455",
                    "Supply of Electrical Distribution Boards (JTC Facilities)",
                    "JTC Corporation", "GeBIZ", TenderStatus.Interested, 95000m, At(2, 16),
                    "Closing soon — small supply-only job. Good margin.",
                    new[]
                    {
                        "Supply 40 nos. 12-way TPN distribution boards",
                        "All boards to be SS-compliant and CE-marked",
                        "Delivery to 4 JTC sites within 6 weeks of award",
                    }),
                NewTender("TB/OT/2026/0091",
                    "Structured Cabling for New School Campus",
                    "Ministry of Education", "Tenderboard", TenderStatus.Drafting, 410000m, At(18),
                    "Greenfield campus, full Cat6A backbone + WiFi6 APs.",
                    new[]
                    {
                        "Supply and install Cat6A structured cabling for 600 data points",
                        "Fibre backbone between 6 blocks",
                        "Wireless access points (WiFi 6) with controller",
                        "Certification testing and as-built documentation",
                    }),
                NewTender("GeBiz/ITQ/2026/0203",
                    "Air-Conditioning Servicing Contract (Parks & Pavilions)",
                    "National Parks Board", "GeBIZ", TenderStatus.Won, 220000m, At(-30),
                    "Awarded. 1-year servicing contract.",
                    new[]
                    {
                        "Quarterly servicing of 180 split and cassette units",
                        "Chemical wash twice yearly",
                        "Replacement of consumables included",
                    }));

            await db.SaveChangesAsync();
        }

        private static Tender NewTender(
            string reference, string title, string agency, string source, string status,
            decimal estValue, DateTime closingAt, string notes, string[] specs)
        {
            var now = DateTime.UtcNow;
            var tender = new Tender
            {
                Reference = reference,
                Title = title,
                Agency = agency,
                Source = source,
                Status = status,
                EstValue = estValue,
                ClosingAt = closingAt,
                Notes = notes,
                CreatedAt = now,
                UpdatedAt = now,
            };

            for (var i = 0; i < specs.Length; i++)
                tender.Specs.Add(new TenderSpec { Ordinal = i, Text = specs[i] });

            return tender;
        }

        private static async Task SeedInventoryAsync(AppDbContext db)
        {
            if (await db.InventoryItems.AnyAsync()) return;

            var catalog = new (string Name, string Category, string Unit, decimal Cost, string Ref, int AgeDays)[]
            {
                ("LED Batten 18W (with motion sensor)", "Lighting", "each", 38.0m, "GeBiz/ITQ/2025/0991", -40),
                ("LED High Bay 150W", "Lighting", "each", 78.0m, "GeBiz/ITQ/2025/0991", -40),
                ("Distribution Board 12-way TPN", "Switchgear", "each", 240.0m, "GeBiz/ITQ/2025/0774", -120),
                ("MCB 32A", "Switchgear", "each", 18.0m, "GeBiz/ITQ/2025/0774", -120),
                ("4MP IP Dome Camera", "Security", "each", 145.0m, "SESAMI/RFQ/2025/0612", -75),
                ("Network Video Recorder 32-ch", "Security", "each", 1180.0m, "SESAMI/RFQ/2025/0612", -75),
                ("Cat6A Cable (305m box)", "Cabling", "box", 165.0m, "TB/OT/2025/0044", -30),
                ("PoE Switch 24-port", "Cabling", "each", 520.0m, "TB/OT/2025/0044", -30),
                ("Cable Tray 100mm (galvanised)", "Cabling", "meter", 12.5m, "GeBiz/ITQ/2025/0991", -40),
                ("AC Cassette Unit 3.5kW", "ACMV", "each", 1250.0m, "GeBiz/ITQ/2026/0203", -30),
                ("Exit Light (LED, self-test)", "Lighting", "each", 42.0m, "GeBiz/ITQ/2025/0991", -40),
                ("Smoke Detector (addressable)", "Fire Safety", "each", 55.0m, "GeBiz/ITQ/2025/0810", -95),
            };

            foreach (var row in catalog)
            {
                var when = DateTime.UtcNow.AddDays(row.AgeDays);
                var item = new InventoryItem
                {
                    Name = row.Name,
                    Category = row.Category,
                    Unit = row.Unit,
                    LastTenderRef = row.Ref,
                    IsActive = true,
                    CreatedAt = when,
                    UpdatedAt = when,
                };

                // An older price plus the current one, so the history view has something to show.
                item.Prices.Add(new PriceHistory
                {
                    UnitCost = decimal.Round(row.Cost * 0.94m, 2),
                    EffectiveFrom = when.AddDays(-180),
                    SourceTenderRef = row.Ref,
                    CreatedAt = when.AddDays(-180),
                });
                item.Prices.Add(new PriceHistory
                {
                    UnitCost = row.Cost,
                    EffectiveFrom = when,
                    SourceTenderRef = row.Ref,
                    CreatedAt = when,
                });

                db.InventoryItems.Add(item);
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedLabourAsync(AppDbContext db)
        {
            if (await db.LabourRates.AnyAsync()) return;

            var roles = new (string Role, decimal Rate)[]
            {
                ("Licensed Electrician (EMA)", 45.0m),
                ("Electrical Technician", 32.0m),
                ("M&E Engineer", 65.0m),
                ("Project Supervisor", 55.0m),
                ("General Worker", 22.0m),
                ("CCTV / Network Specialist", 48.0m),
            };

            foreach (var row in roles)
            {
                var labour = new LabourRate
                {
                    Role = row.Role,
                    Unit = "hour",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-200),
                    UpdatedAt = DateTime.UtcNow.AddDays(-60),
                };

                labour.Rates.Add(new LabourRateHistory
                {
                    HourlyRate = decimal.Round(row.Rate * 0.95m, 2),
                    EffectiveFrom = DateTime.UtcNow.AddDays(-200),
                    CreatedAt = DateTime.UtcNow.AddDays(-200),
                });
                labour.Rates.Add(new LabourRateHistory
                {
                    HourlyRate = row.Rate,
                    EffectiveFrom = DateTime.UtcNow.AddDays(-60),
                    CreatedAt = DateTime.UtcNow.AddDays(-60),
                });

                db.LabourRates.Add(labour);
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedDeadlinesAsync(AppDbContext db)
        {
            if (await db.TenderDeadlines.AnyAsync()) return;

            var tenders = await db.Tenders.ToDictionaryAsync(t => t.Reference, t => t);

            void Add(string reference, string title, string type, DateTime dueAt, string priority)
            {
                if (!tenders.TryGetValue(reference, out var tender)) return;

                db.TenderDeadlines.Add(new TenderDeadline
                {
                    TenderId = tender.Id,
                    Title = title,
                    Type = type,
                    DueAt = dueAt,
                    Priority = priority,
                    AddedToCalendar = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
            }

            Add("GeBiz/ITQ/2026/0455", "JTC Distribution Boards — Tender Closing",
                DeadlineType.Closing, At(2, 16), DeadlinePriority.High);
            Add("GeBiz/ITQ/2026/0418", "HDB LED Lighting — Tender Closing",
                DeadlineType.Closing, At(5, 17), DeadlinePriority.High);
            Add("GeBiz/ITT/2026/0377", "PA CCTV Upgrade — Site Briefing",
                DeadlineType.Briefing, At(3, 10), DeadlinePriority.Medium);
            Add("GeBiz/ITT/2026/0377", "PA CCTV Upgrade — Tender Closing",
                DeadlineType.Closing, At(12, 17), DeadlinePriority.Medium);
            Add("TB/OT/2026/0091", "MOE Structured Cabling — Clarification Deadline",
                DeadlineType.Clarification, At(8, 12), DeadlinePriority.Low);
            Add("TB/OT/2026/0091", "MOE Structured Cabling — Tender Closing",
                DeadlineType.Closing, At(18, 17), DeadlinePriority.Medium);

            await db.SaveChangesAsync();
        }

        private static async Task SeedTemplatesAsync(AppDbContext db)
        {
            if (await db.DocumentTemplates.AnyAsync()) return;

            db.DocumentTemplates.AddRange(
                new DocumentTemplate
                {
                    Name = "Technical Proposal",
                    Section = "1. Company Introduction",
                    Ordinal = 1,
                    BodyTemplate =
                        "{{company}} (UEN {{uen}}) is a bizSAFE Level 3 certified Mechanical & Electrical " +
                        "contractor holding a valid EMA Class licence. Over the past 12 years we have " +
                        "delivered electrical, lighting and security projects for public-sector clients " +
                        "including HDB, JTC, NParks and {{agency}}.",
                },
                new DocumentTemplate
                {
                    Name = "Technical Proposal",
                    Section = "2. Understanding of Requirements",
                    Ordinal = 2,
                    BodyTemplate =
                        "We understand that {{agency}} requires the following under reference {{reference}}:\n\n" +
                        "{{specs}}\n\nOur proposal below addresses each requirement in full, with a delivery " +
                        "approach designed to minimise disruption and meet the stipulated completion period.",
                },
                new DocumentTemplate
                {
                    Name = "Technical Proposal",
                    Section = "3. Proposed Methodology",
                    Ordinal = 3,
                    BodyTemplate =
                        "Works will be executed in phases — survey, isolation, removal/installation, then " +
                        "testing and commissioning. Each phase is supervised by a dedicated Project Supervisor " +
                        "and licensed personnel. All works comply with SS 531, the relevant BCA Green Mark " +
                        "criteria, and the Workplace Safety & Health Act.",
                },
                new DocumentTemplate
                {
                    Name = "Technical Proposal",
                    Section = "4. Compliance & Certifications",
                    Ordinal = 4,
                    BodyTemplate =
                        "• bizSAFE Level 3\n• EMA Electrical Worker / Class Licence\n" +
                        "• ISO 9001:2015 Quality Management\n• Workplace Safety & Health (WSH) compliant\n\n" +
                        "A 12-month defects liability period is offered.",
                },
                new DocumentTemplate
                {
                    Name = "Technical Proposal",
                    Section = "5. Project Team & Experience",
                    Ordinal = 5,
                    BodyTemplate =
                        "The project will be led by an M&E Engineer supported by licensed electricians and " +
                        "technicians. Relevant recent references include similar works for HDB town councils " +
                        "and institutional clients, available on request. Enquiries: {{email}} / {{phone}}.",
                });

            await db.SaveChangesAsync();
        }

        private static async Task SeedQuotationsAsync(AppDbContext db)
        {
            if (await db.Quotations.AnyAsync()) return;

            var tenders = await db.Tenders.ToDictionaryAsync(t => t.Reference, t => t);
            var items = await db.InventoryItems.ToDictionaryAsync(i => i.Name, i => i);
            var labour = await db.LabourRates.ToDictionaryAsync(l => l.Role, l => l);
            var admin = await db.Users.FirstOrDefaultAsync(u => u.Role == Roles.Admin);

            if (!tenders.TryGetValue("GeBiz/ITQ/2026/0455", out var jtc) ||
                !tenders.TryGetValue("GeBiz/ITQ/2026/0418", out var hdb))
                return;

            var year = DateTime.UtcNow.Year;

            var verified = new Quotation
            {
                QuoteNo = $"TQ-Q{year}-001",
                TenderId = jtc.Id,
                Title = "Supply of Electrical Distribution Boards",
                Client = jtc.Agency,
                Status = QuotationStatus.Verified,
                Version = 1,
                MarkupPct = 15m,
                GstPct = 9m,
                Verified = true,
                VerifiedBy = admin?.Name ?? "Admin User",
                VerifiedAt = DateTime.UtcNow.AddDays(-1),
                CreatedByUserId = admin?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
            };

            AddEquipmentLine(verified, 0, "Distribution Board 12-way TPN", 40m, "each", 240.0m, items);
            AddEquipmentLine(verified, 1, "MCB 32A", 480m, "each", 18.0m, items);
            AddLabourLine(verified, 2, "Electrical Technician (delivery & testing)", 48m, 32.0m,
                labour, "Electrical Technician");

            verified.Signoffs.Add(new QuotationSignoff
            {
                UserId = admin?.Id,
                UserName = admin?.Name ?? "Admin User",
                QuoteVersion = 1,
                SignedAt = DateTime.UtcNow.AddDays(-1),
            });

            var draft = new Quotation
            {
                QuoteNo = $"TQ-Q{year}-002",
                TenderId = hdb.Id,
                Title = "LED Lighting Retrofit at HDB Blocks",
                Client = hdb.Agency,
                Status = QuotationStatus.Draft,
                Version = 1,
                MarkupPct = 18m,
                GstPct = 9m,
                CreatedByUserId = admin?.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
            };

            AddEquipmentLine(draft, 0, "LED Batten 18W (with motion sensor)", 1200m, "each", 38.0m, items);
            AddEquipmentLine(draft, 1, "Cable Tray 100mm (galvanised)", 350m, "meter", 12.5m, items);
            AddLabourLine(draft, 2, "Licensed Electrician (EMA)", 320m, 45.0m, labour, "Licensed Electrician (EMA)");
            AddLabourLine(draft, 3, "General Worker", 480m, 22.0m, labour, "General Worker");

            Recalculate(verified);
            Recalculate(draft);

            db.Quotations.AddRange(verified, draft);
            await db.SaveChangesAsync();
        }

        private static void AddEquipmentLine(
            Quotation quote, int ordinal, string description,
            decimal qty, string unit, decimal unitPrice, IDictionary<string, InventoryItem> items)
        {
            items.TryGetValue(description, out var item);
            quote.Lines.Add(new QuotationLine
            {
                Ordinal = ordinal,
                Kind = QuotationLineKind.Equipment,
                Description = description,
                Qty = qty,
                Unit = unit,
                UnitPrice = unitPrice,
                InventoryItemId = item?.Id,
            });
        }

        private static void AddLabourLine(
            Quotation quote, int ordinal, string description, decimal qty, decimal rate,
            IDictionary<string, LabourRate> labour, string roleKey)
        {
            labour.TryGetValue(roleKey, out var role);
            quote.Lines.Add(new QuotationLine
            {
                Ordinal = ordinal,
                Kind = QuotationLineKind.Labour,
                Description = description,
                Qty = qty,
                Unit = "hour",
                UnitPrice = rate,
                LabourRateId = role?.Id,
            });
        }

        private static void Recalculate(Quotation quote)
        {
            var subtotal = quote.Lines.Sum(l => l.Qty * l.UnitPrice);
            var preGst = subtotal + (subtotal * quote.MarkupPct / 100m);
            var total = preGst + (preGst * quote.GstPct / 100m);

            quote.Subtotal = decimal.Round(subtotal, 2, MidpointRounding.AwayFromZero);
            quote.Total = decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        }

        private static async Task SeedDraftsAsync(AppDbContext db)
        {
            if (await db.TenderDocuments.AnyAsync()) return;

            var tenders = await db.Tenders.ToDictionaryAsync(t => t.Reference, t => t);

            if (tenders.TryGetValue("GeBiz/ITQ/2026/0418", out var hdb))
            {
                var doc = new TenderDocument
                {
                    TenderId = hdb.Id,
                    Title = "Technical Proposal — LED Lighting Retrofit",
                    Type = "Technical Proposal",
                    Status = DocumentStatus.InReview,
                    Version = 3,
                    CreatedAt = DateTime.UtcNow.AddDays(-6),
                    UpdatedAt = DateTime.UtcNow.AddDays(-1),
                };

                doc.Sections.Add(new TenderDocumentSection
                {
                    Ordinal = 0,
                    Heading = "1. Company Introduction",
                    Body = "TenderQuick Engineering Pte Ltd is a bizSAFE Level 3 certified M&E contractor with " +
                           "over 12 years of experience delivering electrical and lighting projects for " +
                           "public-sector clients including HDB, JTC and NParks.",
                });
                doc.Sections.Add(new TenderDocumentSection
                {
                    Ordinal = 1,
                    Heading = "2. Understanding of Requirements",
                    Body = "We understand the Authority requires the supply and installation of 1,200 nos. LED " +
                           "batten fittings with integrated motion sensors across 14 residential blocks in " +
                           "Tampines, with full compliance to SS 531 and BCA Green Mark.",
                });
                doc.Sections.Add(new TenderDocumentSection
                {
                    Ordinal = 2,
                    Heading = "3. Proposed Methodology",
                    Body = "Works will be executed block-by-block to minimise disruption to residents. Each " +
                           "phase comprises survey, isolation, removal of existing fittings, installation, " +
                           "testing and commissioning.",
                });

                db.TenderDocuments.Add(doc);
            }

            if (tenders.TryGetValue("TB/OT/2026/0091", out var moe))
            {
                var doc = new TenderDocument
                {
                    TenderId = moe.Id,
                    Title = "Technical Proposal — Structured Cabling (MOE Campus)",
                    Type = "Technical Proposal",
                    Status = DocumentStatus.Draft,
                    Version = 1,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                };

                doc.Sections.Add(new TenderDocumentSection
                {
                    Ordinal = 0,
                    Heading = "1. Company Introduction",
                    Body = "TenderQuick Engineering Pte Ltd brings proven structured-cabling delivery across " +
                           "education and institutional projects.",
                });

                db.TenderDocuments.Add(doc);
            }

            await db.SaveChangesAsync();
        }

        private static async Task SeedMemoryAsync(AppDbContext db)
        {
            if (await db.AiMemories.AnyAsync()) return;

            var memory = new AiMemory
            {
                UserId = null,
                SamplesLearned = 14,
                LastUpdatedAt = DateTime.UtcNow.AddDays(-1),
            };

            memory.Preferences.Add(new AiPreference
            {
                Text = "Open every proposal with the bizSAFE Level 3 and EMA Class licence statement",
                Confidence = 0.94m,
                Source = "Applied in 11 of your last 12 tenders",
                TimesApplied = 11,
            });
            memory.Preferences.Add(new AiPreference
            {
                Text = "Use formal tone with numbered clauses (1., 1.1, 1.2)",
                Confidence = 0.89m,
                Source = "Matched your edits on 9 drafts",
                TimesApplied = 9,
            });
            memory.Preferences.Add(new AiPreference
            {
                Text = "Always cite SS 531 and BCA Green Mark for lighting works",
                Confidence = 0.86m,
                Source = "You added this to 6 lighting tenders",
                TimesApplied = 6,
            });
            memory.Preferences.Add(new AiPreference
            {
                Text = "Phase works block-by-block to minimise resident disruption",
                Confidence = 0.78m,
                Source = "Learned from HDB/Town Council submissions",
                TimesApplied = 4,
            });

            db.AiMemories.Add(memory);
            await db.SaveChangesAsync();
        }
    }
}
