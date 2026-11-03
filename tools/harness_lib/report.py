"""Render independent harness checks as a compact Markdown table."""


def render(results):
    lines = ["| Check | Result | Details |", "|---|---|---|"]
    for name, result in results.items():
        details = ", ".join(result.get("failures", [])) or result.get("error", "")
        details = str(details).replace("|", "\\|").replace("\n", " ")
        lines.append(f"| {name} | {'PASS' if result['passed'] else 'FAIL'} | {details} |")
    return "\n".join(lines) + "\n"
