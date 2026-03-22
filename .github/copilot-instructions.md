# GitHub Copilot Custom Instructions

## System Configuration
* **thinkingLevel**: "MEDIUM"

## UI & Component Standards
* **Exclusive DevExpress Usage**: You must exclusively use DevExpress controls for all UI components and visual elements within this solution. Do not use standard HTML elements or generic built-in framework components if a DevExpress equivalent exists.

## Code Evaluation & Generation Rules
* **Documentation Priority**: When evaluating code, generating solutions, or providing suggestions, you must always prioritize and adhere to the official DevExpress documentation. 
* **API Preference**: Rely on DevExpress-specific APIs, structures, and best practices over generic C# workarounds whenever applicable.
* # Output Length and Formatting Constraints

1. **Keep Code Snippets Extremely Short:** Never output more than 15 to 20 lines of code in a single response.
2. **Do Not Rewrite Entire Files:** If modifying existing code, DO NOT output the entire file or large blocks of unchanged code. 
3. **Use Truncation Comments:** Always use comments like `// ... existing code ...` or `` to skip over code that does not need to be changed.
4. **Focus on the Diff:** Only provide the exact lines that need to be added, modified, or removed.
5. **Ask Before Expanding:** If a solution absolutely requires more than 20 lines of code, provide the first 15 lines, explain the rest conceptually, and ask the user if they want the next chunk.