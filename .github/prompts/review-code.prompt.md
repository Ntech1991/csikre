---
name: Review Code
description: Review selected code, a supplied file, or the pending Git diff for bugs, regressions, risks, and missing tests.
argument-hint: Optional review focus or risk area
agent: agent
---
Review the selected code, the file/context supplied in the chat, or the pending Git diff when no narrower target is provided, as a senior code reviewer.

Priorities:
- Find concrete bugs, behavioral regressions, security risks, reliability issues, and missing tests.
- Trace the nearby owning code and call sites when needed; do not review isolated syntax without understanding its behavior.
- Follow existing repository patterns and identify assumptions that are not enforced.
- Do not suggest unrelated refactors or style changes unless they create a real defect or maintenance risk.

For each finding:
- Start with severity: Critical, High, Medium, or Low.
- Explain the concrete failure mode and why it matters.
- Include a workspace-relative clickable file link with the relevant line when available.
- Give a concise remediation direction, but do not edit files unless explicitly asked.

Response format:
1. Findings first, ordered by severity.
2. Open questions or assumptions.
3. A brief change summary only if reviewing a proposed change.
4. Remaining test gaps or residual risk.

If there are no findings, say so clearly and still mention meaningful test gaps or residual risk. Keep the review concise and evidence-based.

Optional review focus: ${input:focus:What should this review emphasize?}
