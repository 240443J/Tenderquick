// In-memory mock database for the TenderQuick prototype.
// Shapes mirror the eventual backend so the api/*.js layer can swap to real
// axios calls later without touching any page code.

function iso(daysFromNow, hour = 17, min = 0) {
  const d = new Date()
  d.setDate(d.getDate() + daysFromNow)
  d.setHours(hour, min, 0, 0)
  return d.toISOString()
}

let seq = 5000
export const nextId = () => ++seq

export const COMPANY = {
  name: 'TenderQuick Engineering Pte Ltd',
  uen: '202418842K',
  address: '71 Ubi Road 1, #05-32 Oxley BizHub, Singapore 408732',
  email: 'tenders@tenderquick.sg',
  phone: '+65 6555 0182',
  gstReg: 'M2-0123456-7',
}

export const db = {
  user: { id: 1, name: 'Jing Yang', email: 'jingyanglim23@gmail.com', role: 'Admin' },

  calendar: { connected: false, account: null },

  tenders: [
    {
      id: 101,
      reference: 'GeBiz/ITQ/2026/0418',
      title: 'Supply & Installation of LED Lighting at HDB Blocks (Tampines)',
      agency: 'Housing & Development Board',
      source: 'GeBiz',
      status: 'Drafting',
      estValue: 185000,
      closingAt: iso(5),
      createdAt: iso(-9),
      updatedAt: iso(-1),
      notes: 'Retrofit of common-area lighting across 14 blocks. T8 LED, motion sensors at staircases.',
      specs: [
        'Supply and install 1,200 nos. LED batten 18W with motion sensor at staircases',
        'Replace existing fluorescent fittings at common corridors',
        'All works to comply with SS 531 and BCA Green Mark requirements',
        'Contractor to hold valid bizSAFE Level 3 and EMA Class licence',
        '12-month defects liability period',
      ],
    },
    {
      id: 102,
      reference: 'GeBiz/ITT/2026/0377',
      title: 'CCTV System Upgrade at Community Clubs (Central Region)',
      agency: "People's Association",
      source: 'GeBiz',
      status: 'Interested',
      estValue: 320000,
      closingAt: iso(12),
      createdAt: iso(-4),
      updatedAt: iso(-4),
      notes: 'Upgrade analogue CCTV to 4MP IP across 8 CCs. NVR with 30-day retention.',
      specs: [
        'Supply and install 96 nos. 4MP IP dome cameras across 8 community clubs',
        'Network video recorders with minimum 30-day footage retention',
        'Integration with existing PA security operations centre',
        'Cat6 structured cabling and PoE switches',
      ],
    },
    {
      id: 103,
      reference: 'SESAMI/RFQ/2026/1182',
      title: 'Periodic M&E Maintenance for Polyclinics',
      agency: 'National Healthcare Group',
      source: 'Sesami',
      status: 'Submitted',
      estValue: 540000,
      closingAt: iso(-2),
      createdAt: iso(-25),
      updatedAt: iso(-3),
      notes: '2-year M&E maintenance, quarterly servicing. Submitted, awaiting outcome.',
      specs: [
        'Quarterly preventive maintenance of electrical, ACMV and plumbing systems',
        'Provision of standby manpower for fault response within 4 hours',
        '24/7 emergency call-out coverage',
      ],
    },
    {
      id: 104,
      reference: 'GeBiz/ITQ/2026/0455',
      title: 'Supply of Electrical Distribution Boards (JTC Facilities)',
      agency: 'JTC Corporation',
      source: 'GeBiz',
      status: 'Interested',
      estValue: 95000,
      closingAt: iso(2),
      createdAt: iso(-2),
      updatedAt: iso(-1),
      notes: 'Closing soon — small supply-only job. Good margin.',
      specs: [
        'Supply 40 nos. 12-way TPN distribution boards',
        'All boards to be SS-compliant and CE-marked',
        'Delivery to 4 JTC sites within 6 weeks of award',
      ],
    },
    {
      id: 105,
      reference: 'TB/OT/2026/0091',
      title: 'Structured Cabling for New School Campus',
      agency: 'Ministry of Education',
      source: 'TenderBoard',
      status: 'Drafting',
      estValue: 410000,
      closingAt: iso(18),
      createdAt: iso(-6),
      updatedAt: iso(-2),
      notes: 'Greenfield campus, full Cat6A backbone + WiFi6 APs.',
      specs: [
        'Supply and install Cat6A structured cabling for 600 data points',
        'Fibre backbone between 6 blocks',
        'Wireless access points (WiFi 6) with controller',
        'Certification testing and as-built documentation',
      ],
    },
    {
      id: 106,
      reference: 'GeBiz/ITQ/2026/0203',
      title: 'Air-Conditioning Servicing Contract (Parks & Pavilions)',
      agency: 'National Parks Board',
      source: 'GeBiz',
      status: 'Won',
      estValue: 220000,
      closingAt: iso(-30),
      createdAt: iso(-60),
      updatedAt: iso(-30),
      notes: 'Awarded. 1-year servicing contract.',
      specs: [
        'Quarterly servicing of 180 split and cassette units',
        'Chemical wash twice yearly',
        'Replacement of consumables included',
      ],
    },
  ],

  equipment: [
    { id: 201, name: 'LED Batten 18W (with motion sensor)', category: 'Lighting', unit: 'each', unitCost: 38.0, lastTenderRef: 'GeBiz/ITQ/2025/0991', updatedAt: iso(-40) },
    { id: 202, name: 'LED High Bay 150W', category: 'Lighting', unit: 'each', unitCost: 78.0, lastTenderRef: 'GeBiz/ITQ/2025/0991', updatedAt: iso(-40) },
    { id: 203, name: 'Distribution Board 12-way TPN', category: 'Switchgear', unit: 'each', unitCost: 240.0, lastTenderRef: 'GeBiz/ITQ/2025/0774', updatedAt: iso(-120) },
    { id: 204, name: 'MCB 32A', category: 'Switchgear', unit: 'each', unitCost: 18.0, lastTenderRef: 'GeBiz/ITQ/2025/0774', updatedAt: iso(-120) },
    { id: 205, name: '4MP IP Dome Camera', category: 'Security', unit: 'each', unitCost: 145.0, lastTenderRef: 'SESAMI/RFQ/2025/0612', updatedAt: iso(-75) },
    { id: 206, name: 'Network Video Recorder 32-ch', category: 'Security', unit: 'each', unitCost: 1180.0, lastTenderRef: 'SESAMI/RFQ/2025/0612', updatedAt: iso(-75) },
    { id: 207, name: 'Cat6A Cable (305m box)', category: 'Cabling', unit: 'box', unitCost: 165.0, lastTenderRef: 'TB/OT/2025/0044', updatedAt: iso(-30) },
    { id: 208, name: 'PoE Switch 24-port', category: 'Cabling', unit: 'each', unitCost: 520.0, lastTenderRef: 'TB/OT/2025/0044', updatedAt: iso(-30) },
    { id: 209, name: 'Cable Tray 100mm (galvanised)', category: 'Cabling', unit: 'meter', unitCost: 12.5, lastTenderRef: 'GeBiz/ITQ/2025/0991', updatedAt: iso(-40) },
    { id: 210, name: 'AC Cassette Unit 3.5kW', category: 'ACMV', unit: 'each', unitCost: 1250.0, lastTenderRef: 'GeBiz/ITQ/2026/0203', updatedAt: iso(-30) },
    { id: 211, name: 'Exit Light (LED, self-test)', category: 'Lighting', unit: 'each', unitCost: 42.0, lastTenderRef: 'GeBiz/ITQ/2025/0991', updatedAt: iso(-40) },
    { id: 212, name: 'Smoke Detector (addressable)', category: 'Fire Safety', unit: 'each', unitCost: 55.0, lastTenderRef: 'GeBiz/ITQ/2025/0810', updatedAt: iso(-95) },
  ],

  labour: [
    { id: 301, role: 'Licensed Electrician (EMA)', unit: 'hour', rate: 45.0 },
    { id: 302, role: 'Electrical Technician', unit: 'hour', rate: 32.0 },
    { id: 303, role: 'M&E Engineer', unit: 'hour', rate: 65.0 },
    { id: 304, role: 'Project Supervisor', unit: 'hour', rate: 55.0 },
    { id: 305, role: 'General Worker', unit: 'hour', rate: 22.0 },
    { id: 306, role: 'CCTV / Network Specialist', unit: 'hour', rate: 48.0 },
  ],

  quotations: [
    {
      id: 401,
      quoteNo: 'TQ-Q2026-018',
      tenderId: 104,
      tenderRef: 'GeBiz/ITQ/2026/0455',
      title: 'Supply of Electrical Distribution Boards',
      client: 'JTC Corporation',
      status: 'Verified',
      verified: true,
      verifiedBy: 'Jing Yang',
      verifiedAt: iso(-1),
      markupPct: 15,
      gstPct: 9,
      createdAt: iso(-2),
      lineItems: [
        { id: 1, kind: 'equipment', desc: 'Distribution Board 12-way TPN', qty: 40, unit: 'each', unitPrice: 240.0 },
        { id: 2, kind: 'equipment', desc: 'MCB 32A', qty: 480, unit: 'each', unitPrice: 18.0 },
        { id: 3, kind: 'labour', desc: 'Electrical Technician (delivery & testing)', qty: 48, unit: 'hour', unitPrice: 32.0 },
      ],
    },
    {
      id: 402,
      quoteNo: 'TQ-Q2026-021',
      tenderId: 101,
      tenderRef: 'GeBiz/ITQ/2026/0418',
      title: 'LED Lighting Retrofit at HDB Blocks',
      client: 'Housing & Development Board',
      status: 'Draft',
      verified: false,
      verifiedBy: null,
      verifiedAt: null,
      markupPct: 18,
      gstPct: 9,
      createdAt: iso(-1),
      lineItems: [
        { id: 1, kind: 'equipment', desc: 'LED Batten 18W (with motion sensor)', qty: 1200, unit: 'each', unitPrice: 38.0 },
        { id: 2, kind: 'equipment', desc: 'Cable Tray 100mm (galvanised)', qty: 350, unit: 'meter', unitPrice: 12.5 },
        { id: 3, kind: 'labour', desc: 'Licensed Electrician (EMA)', qty: 320, unit: 'hour', unitPrice: 45.0 },
        { id: 4, kind: 'labour', desc: 'General Worker', qty: 480, unit: 'hour', unitPrice: 22.0 },
      ],
    },
  ],

  drafts: [
    {
      id: 501,
      title: 'Technical Proposal — LED Lighting Retrofit',
      tenderId: 101,
      tenderRef: 'GeBiz/ITQ/2026/0418',
      type: 'Technical Proposal',
      status: 'In Review',
      version: 3,
      updatedAt: iso(-1),
      sections: [
        { heading: '1. Company Introduction', body: 'TenderQuick Engineering Pte Ltd is a bizSAFE Level 3 certified M&E contractor with over 12 years of experience delivering electrical and lighting projects for public-sector clients including HDB, JTC and NParks.' },
        { heading: '2. Understanding of Requirements', body: 'We understand the Authority requires the supply and installation of 1,200 nos. LED batten fittings with integrated motion sensors across 14 residential blocks in Tampines, with full compliance to SS 531 and BCA Green Mark.' },
        { heading: '3. Proposed Methodology', body: 'Works will be executed block-by-block to minimise disruption to residents. Each phase comprises survey, isolation, removal of existing fittings, installation, testing and commissioning.' },
      ],
    },
    {
      id: 502,
      title: 'Technical Proposal — Structured Cabling (MOE Campus)',
      tenderId: 105,
      tenderRef: 'TB/OT/2026/0091',
      type: 'Technical Proposal',
      status: 'Draft',
      version: 1,
      updatedAt: iso(-2),
      sections: [
        { heading: '1. Company Introduction', body: 'TenderQuick Engineering Pte Ltd brings proven structured-cabling delivery across education and institutional projects.' },
      ],
    },
  ],

  scrapeSources: [
    { id: 'gebiz', label: 'GeBiz', enabled: true, status: 'connected', note: 'Government Electronic Business' },
    { id: 'sesami', label: 'Sesami', enabled: false, status: 'beta', note: 'Healthcare & institutional' },
    { id: 'tenderboard', label: 'TenderBoard', enabled: false, status: 'beta', note: 'Aggregated public tenders' },
  ],

  // Candidate tenders the scraper "discovers" — not yet imported into the workspace.
  scrapeResults: [
    { id: 901, reference: 'GeBiz/ITQ/2026/0501', title: 'Supply & Installation of LED Lighting at Bus Interchanges', agency: 'Land Transport Authority', source: 'gebiz', publishedAt: iso(-1), closingAt: iso(9), estValueRange: 'S$150k – S$250k', relevance: 96, matched: ['LED', 'lighting', 'installation'], summary: 'Retrofit of high-bay and area lighting across 6 bus interchanges to LED with smart controls.', imported: false },
    { id: 902, reference: 'GeBiz/ITT/2026/0489', title: 'Electrical Rewiring Works at Hawker Centres', agency: 'National Environment Agency', source: 'gebiz', publishedAt: iso(-2), closingAt: iso(14), estValueRange: 'S$300k – S$450k', relevance: 88, matched: ['electrical', 'installation'], summary: 'Rewiring and distribution board upgrade across 5 hawker centres.', imported: false },
    { id: 903, reference: 'GeBiz/ITQ/2026/0495', title: 'CCTV & Access Control at Town Council Estates', agency: 'Tampines Town Council', source: 'gebiz', publishedAt: iso(-1), closingAt: iso(7), estValueRange: 'S$200k – S$350k', relevance: 84, matched: ['CCTV', 'security'], summary: 'Supply, install and maintain IP CCTV and door access systems.', imported: false },
    { id: 904, reference: 'GeBiz/ITQ/2026/0510', title: 'Supply of Distribution Boards & Switchgear', agency: 'PUB, Singapore’s National Water Agency', source: 'gebiz', publishedAt: iso(0), closingAt: iso(4), estValueRange: 'S$80k – S$120k', relevance: 81, matched: ['distribution board', 'switchgear'], summary: 'Supply-only of LV switchgear and distribution boards for pumping stations.', imported: false },
    { id: 905, reference: 'GeBiz/ITT/2026/0478', title: 'ACMV Maintenance for Sports Facilities', agency: 'Sport Singapore', source: 'gebiz', publishedAt: iso(-3), closingAt: iso(20), estValueRange: 'S$400k – S$600k', relevance: 72, matched: ['ACMV', 'maintenance'], summary: 'Term contract for servicing of air-conditioning and mechanical ventilation systems.', imported: false },
    { id: 906, reference: 'GeBiz/ITQ/2026/0466', title: 'Structured Cabling Upgrade at Public Library', agency: 'National Library Board', source: 'gebiz', publishedAt: iso(-2), closingAt: iso(11), estValueRange: 'S$90k – S$160k', relevance: 69, matched: ['cabling', 'network'], summary: 'Cat6A re-cabling and WiFi refresh at 3 regional libraries.', imported: false },
  ],

  deadlines: [
    { id: 601, tenderId: 104, tenderRef: 'GeBiz/ITQ/2026/0455', title: 'JTC Distribution Boards — Tender Closing', type: 'Closing', dueAt: iso(2, 16, 0), addedToCalendar: false, priority: 'high' },
    { id: 602, tenderId: 101, tenderRef: 'GeBiz/ITQ/2026/0418', title: 'HDB LED Lighting — Tender Closing', type: 'Closing', dueAt: iso(5, 17, 0), addedToCalendar: true, priority: 'high' },
    { id: 603, tenderId: 102, tenderRef: 'GeBiz/ITT/2026/0377', title: 'PA CCTV Upgrade — Site Briefing', type: 'Briefing', dueAt: iso(3, 10, 0), addedToCalendar: false, priority: 'medium' },
    { id: 604, tenderId: 102, tenderRef: 'GeBiz/ITT/2026/0377', title: 'PA CCTV Upgrade — Tender Closing', type: 'Closing', dueAt: iso(12, 17, 0), addedToCalendar: false, priority: 'medium' },
    { id: 605, tenderId: 105, tenderRef: 'TB/OT/2026/0091', title: 'MOE Structured Cabling — Clarification Deadline', type: 'Clarification', dueAt: iso(8, 12, 0), addedToCalendar: false, priority: 'low' },
    { id: 606, tenderId: 105, tenderRef: 'TB/OT/2026/0091', title: 'MOE Structured Cabling — Tender Closing', type: 'Closing', dueAt: iso(18, 17, 0), addedToCalendar: false, priority: 'medium' },
  ],

  audit: [
    { id: 701, action: 'Quotation verified', entity: 'TQ-Q2026-018', user: 'Jing Yang', at: iso(-1, 14, 20) },
    { id: 702, action: 'AI draft regenerated', entity: 'Technical Proposal — LED Lighting', user: 'AI Assistant', at: iso(-1, 11, 5) },
    { id: 703, action: 'Tender imported from GeBiz', entity: 'GeBiz/ITQ/2026/0455', user: 'AI Scraper', at: iso(-2, 9, 30) },
  ],

  // The "memory" that makes the AI improve with use — learned drafting preferences.
  draftMemory: {
    samplesLearned: 14,
    lastUpdated: iso(-1),
    preferences: [
      { id: 1, text: 'Open every proposal with the bizSAFE Level 3 and EMA Class licence statement', confidence: 0.94, source: 'Applied in 11 of your last 12 tenders' },
      { id: 2, text: 'Use formal tone with numbered clauses (1., 1.1, 1.2)', confidence: 0.89, source: 'Matched your edits on 9 drafts' },
      { id: 3, text: 'Always cite SS 531 and BCA Green Mark for lighting works', confidence: 0.86, source: 'You added this to 6 lighting tenders' },
      { id: 4, text: 'Phase works block-by-block to minimise resident disruption', confidence: 0.78, source: 'Learned from HDB/Town Council submissions' },
    ],
  },
}

export function logAudit(action, entity, user = 'Jing Yang') {
  db.audit.unshift({ id: nextId(), action, entity, user, at: new Date().toISOString() })
}
