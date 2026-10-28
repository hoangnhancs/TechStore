---
name: ship
description: "Chạy pipeline architect → coder → tester → deployer cho 1 task/feature cụ thể trong TechStore, có điểm dừng chờ người dùng duyệt plan và chờ xác nhận trước khi deploy. Dùng khi người dùng gọi /ship hoặc nói rõ muốn chạy full flow thiết kế-code-test-deploy cho 1 việc cụ thể. Không dùng cho câu hỏi thường, sửa nhỏ 1-2 dòng, hoặc khi người dùng chỉ muốn trao đổi/đọc hiểu code."
---

## Mục đích

Đây là lớp orchestration nối 4 agent đã định nghĩa ở `.claude/agents/` (`architect`, `coder`, `tester`, `deployer`) thành 1 pipeline có thứ tự, có điều kiện dừng rõ ràng, thay vì để orchestrator (phiên chính) tự nhớ phải gọi đúng thứ tự.

Nguyên tắc xuyên suốt: **chạy tuần tự, không song song** (mỗi bước phụ thuộc kết quả bước trước — `run_in_background: false`), **không tự lặp lại vô hạn khi fail** (dừng lại hỏi người dùng thay vì tự retry nhiều lần tốn quota), và **mọi output trung gian được ghi ra file** trong `.claude/runs/<slug>/` để có bằng chứng cụ thể, agent sau đọc file đó thay vì chỉ tin lời kể lại.

## Bước 0 — Chuẩn bị

1. Lấy mô tả task từ `args`. Nếu mơ hồ (không rõ phạm vi, không rõ service nào), dùng `AskUserQuestion` hỏi lại trước khi chạy — đừng đoán rồi chạy cả pipeline sai hướng.
2. Sinh `slug` ngắn gọn kebab-case từ task (vd "thêm idempotency key stripe" → `stripe-idempotency-key`).
3. Tạo thư mục `.claude/runs/<slug>/` (dùng Bash `mkdir -p`, đã có trong `.gitignore`).
4. `git status` — nếu có thay đổi uncommitted không liên quan tới task này, báo cho người dùng trước, đừng để lẫn vào diff của task.

## Bước 1 — Architect (bắt buộc, foreground)

Gọi `Agent({ subagent_type: "architect", prompt: <mô tả task đầy đủ>, run_in_background: false })`.

Sau khi có kết quả: tự tay (phiên chính, dùng Write) ghi plan nhận được vào `.claude/runs/<slug>/plan.md` — **không** nhờ agent `architect` tự ghi, vì nó không có quyền Write.

Dùng `AskUserQuestion` hỏi người dùng duyệt plan, 3 lựa chọn kiểu:
- "Duyệt, tiến hành code (Recommended)"
- "Cần sửa plan trước" → hỏi rõ muốn sửa gì, gọi lại `architect` với feedback đó, quay lại đầu Bước 1
- "Dừng lại, chưa code vội"

Chỉ sang Bước 2 khi người dùng chọn duyệt.

## Bước 2 — Coder (foreground)

Gọi `Agent({ subagent_type: "coder", prompt: <nội dung plan.md + task gốc>, run_in_background: false })`.

Ghi tóm tắt kết quả (file đã đổi, có gì lệch so với plan) vào `.claude/runs/<slug>/coder-report.md`.

Nếu coder tự báo build fail hoặc chưa làm xong theo plan → dừng pipeline tại đây, báo người dùng, không tự sang Bước 3.

## Bước 3 — Tester (foreground)

Gọi `Agent({ subagent_type: "tester", prompt: <nội dung coder-report.md + plan.md>, run_in_background: false })`, dặn rõ trong prompt: **dòng đầu tiên của báo cáo cuối phải là `VERDICT: PASS` hoặc `VERDICT: FAIL`** để orchestrator đọc được chắc chắn, không phải suy luận qua văn bản tự do.

Ghi toàn bộ báo cáo vào `.claude/runs/<slug>/test-report.md`.

**Điều kiện chuyển bước** — đọc đúng dòng `VERDICT`:
- `FAIL` → **dừng pipeline**, trình bày rõ bug cho người dùng (kèm file:line, kịch bản tái hiện lấy từ report). Không tự động gọi lại `coder` để sửa — hỏi người dùng có muốn chạy lại không (tránh vòng lặp fix-test-fix tốn quota không kiểm soát).
- `PASS` → sang Bước 4.
- Không đọc được VERDICT rõ ràng (tester trả lời mập mờ) → coi như FAIL, dừng lại hỏi người dùng.

## Bước 4 — Deployer (chỉ khi tester PASS, luôn hỏi trước)

Trước khi gọi `deployer`, dùng `AskUserQuestion` xác nhận rõ: "Test đã PASS — tiến hành commit (và hỏi lại trước khi push)?" Chỉ gọi khi người dùng đồng ý.

Gọi `Agent({ subagent_type: "deployer", prompt: <tóm tắt task + xác nhận đã PASS test>, run_in_background: false })`.

Nhắc lại trong prompt gọi deployer: chỉ commit, **không tự `git push` hay deploy thật** nếu chưa hỏi lại — deployer.md đã tự có rule này, đây là lớp phòng thủ kép, không phải thay thế.

## Kết thúc

Tóm tắt lại cho người dùng: task đã làm gì, qua được những bước nào, dừng ở đâu nếu có, link tới các file trong `.claude/runs/<slug>/` để họ tự xem lại chi tiết từng bước.
