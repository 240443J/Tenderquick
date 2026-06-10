// Canned "AI" generators for the prototype. No real model is called — these
// produce plausible, deterministic output shaped by the selected tender so the
// drafting / quotation / scraper demos feel alive.
import { db } from './db'

const firstName = (agency) => (agency || 'the Authority').replace(/^(Ministry of|National|Housing & Development Board).*/, (m) => m)

export function generateDraftSections(tender) {
  const specLines = (tender.specs || []).map((s, i) => `   ${i + 1}.${i + 1}  ${s}`).join('\n')
  return [
    {
      heading: '1. Company Introduction',
      body:
        `${db.draftMemory.preferences[0].text.startsWith('Open') ? '' : ''}` +
        `TenderQuick Engineering Pte Ltd (UEN 202418842K) is a bizSAFE Level 3 certified ` +
        `Mechanical & Electrical contractor holding a valid EMA Class licence. Over the past ` +
        `12 years we have delivered electrical, lighting and security projects for public-sector ` +
        `clients including HDB, JTC, NParks and ${tender.agency}.`,
    },
    {
      heading: '2. Understanding of Requirements',
      body:
        `We understand that ${tender.agency} requires the following under reference ` +
        `${tender.reference}:\n\n${specLines}\n\nOur proposal below addresses each requirement ` +
        `in full, with a delivery approach designed to minimise disruption and meet the ` +
        `stipulated completion period.`,
    },
    {
      heading: '3. Proposed Methodology',
      body:
        `Works will be executed in phases — survey, isolation, removal/installation, then ` +
        `testing and commissioning. Each phase is supervised by a dedicated Project Supervisor ` +
        `and licensed personnel. All works comply with SS 531, the relevant BCA Green Mark ` +
        `criteria, and the Workplace Safety & Health Act.`,
    },
    {
      heading: '4. Compliance & Certifications',
      body:
        `• bizSAFE Level 3\n• EMA Electrical Worker / Class Licence\n• ISO 9001:2015 Quality Management\n` +
        `• Workplace Safety & Health (WSH) compliant\n\nA 12-month defects liability period is offered.`,
    },
    {
      heading: '5. Project Team & Experience',
      body:
        `The project will be led by an M&E Engineer supported by licensed electricians and ` +
        `technicians. Relevant recent references include similar works for HDB town councils and ` +
        `institutional clients, available on request.`,
    },
  ]
}

// Builds quotation line items from the tender specs by matching against the
// inventory (equipment + labour) in the mock DB.
export function generateQuotationLines(tender) {
  const find = (re) => db.equipment.find((e) => re.test(e.name))
  const labour = (re) => db.labour.find((l) => re.test(l.role))
  const text = `${tender.title} ${(tender.specs || []).join(' ')}`.toLowerCase()
  const items = []
  let lid = 1
  const push = (kind, src, qty) => {
    if (!src) return
    items.push({
      id: lid++,
      kind,
      desc: kind === 'equipment' ? src.name : src.role,
      qty,
      unit: src.unit,
      unitPrice: kind === 'equipment' ? src.unitCost : src.rate,
    })
  }

  if (/led|lighting|batten/.test(text)) {
    push('equipment', find(/LED Batten/), 1200)
    push('equipment', find(/Cable Tray/), 350)
    push('equipment', find(/Exit Light/), 60)
  }
  if (/cctv|camera|security/.test(text)) {
    push('equipment', find(/IP Dome Camera/), 96)
    push('equipment', find(/Network Video Recorder/), 8)
  }
  if (/cabling|cat6|network|data point/.test(text)) {
    push('equipment', find(/Cat6A Cable/), 12)
    push('equipment', find(/PoE Switch/), 14)
  }
  if (/distribution board|switchgear|electrical/.test(text)) {
    push('equipment', find(/Distribution Board/), 40)
    push('equipment', find(/MCB/), 480)
  }
  if (/air-?con|acmv|cassette|ventilation/.test(text)) {
    push('equipment', find(/AC Cassette/), 24)
  }

  // Labour — always include supervision + the most relevant trade.
  if (/cctv|cabling|network|data/.test(text)) push('labour', labour(/CCTV|Network/), 240)
  else push('labour', labour(/Licensed Electrician/), 320)
  push('labour', labour(/Project Supervisor/), 80)
  push('labour', labour(/General Worker/), 240)

  if (items.length === 0) {
    push('labour', labour(/M&E Engineer/), 40)
    push('labour', labour(/Electrical Technician/), 80)
  }
  return items
}

// Filters the candidate scrape results by keyword and re-ranks relevance.
export function runScrape(keyword, sourceIds) {
  const kw = (keyword || '').trim().toLowerCase()
  const terms = kw.split(/[\s,]+/).filter(Boolean)
  return db.scrapeResults
    .filter((r) => sourceIds.includes(r.source) && !r.imported)
    .map((r) => {
      if (terms.length === 0) return { ...r }
      const hay = `${r.title} ${r.summary} ${r.matched.join(' ')} ${r.agency}`.toLowerCase()
      const hits = terms.filter((t) => hay.includes(t)).length
      const boost = hits > 0 ? Math.min(8 * hits, 18) : -25
      return { ...r, relevance: Math.max(10, Math.min(99, r.relevance + boost)) }
    })
    .sort((a, b) => b.relevance - a.relevance)
}
