const API_URL = "http://localhost:3000/api/hoadon";

// Lấy và hiển thị hóa đơn
async function loadHoaDon() {
  try {
    const res = await fetch(API_URL);
    if (!res.ok) throw new Error('HTTP error ' + res.status);
    const data = await res.json();

    console.log('DEBUG: data fetched from API:', data); // xem console

    const tbody = document.querySelector("#table-hoadon tbody");
    tbody.innerHTML = "";

    if (!Array.isArray(data) || data.length === 0) {
      tbody.innerHTML = `<tr><td colspan="5">Không có hóa đơn</td></tr>`;
      return;
    }

    data.forEach(hd => {
      // Dùng fallback nếu trường không tồn tại, tránh crash
      const id = hd.id ?? '—';
      const masv = hd.masv ?? (hd.sinhvien_id ? `ID:${hd.sinhvien_id}` : '—');
      const hoten = hd.hoten ?? '';
      const hocKy = hd.hoc_ky ?? '—';
      const tong = (typeof hd.tong_tien === 'number') ? hd.tong_tien : Number(hd.tong_tien);
      const tongFormatted = isNaN(tong) ? '—' : tong.toLocaleString('vi-VN') + ' ₫';
      const trangThai = hd.trang_thai ?? '—';

      const tr = document.createElement("tr");
      tr.innerHTML = `
        <td>${id}</td>
        <td>${masv}${hoten ? ' - ' + hoten : ''}</td>
        <td>${hocKy}</td>
        <td style="text-align:right">${tongFormatted}</td>
        <td>${trangThai}</td>
      `;
      tbody.appendChild(tr);
    });
  } catch (err) {
    console.error('Lỗi khi load hóa đơn:', err);
    const tbody = document.querySelector("#table-hoadon tbody");
    tbody.innerHTML = `<tr><td colspan="5">Lỗi khi tải dữ liệu: ${err.message}</td></tr>`;
  }
}

// Thêm hóa đơn
document.getElementById("form-hoadon").addEventListener("submit", async (e) => {
  e.preventDefault();

  const sinhvien_id = document.getElementById("sinhvien_id").value;
  const hoc_ky = document.getElementById("hoc_ky").value;
  const tong_tien = document.getElementById("tong_tien").value;

  try {
    const res = await fetch(API_URL, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ sinhvien_id, hoc_ky, tong_tien })
    });
    if (!res.ok) {
      const txt = await res.text();
      throw new Error(`Server trả lỗi ${res.status}: ${txt}`);
    }
    alert("Thêm hóa đơn thành công!");
    loadHoaDon();
  } catch (err) {
    console.error('Lỗi khi thêm hóa đơn:', err);
    alert("Lỗi khi thêm hóa đơn: " + err.message);
  }
});

// chạy lần đầu
loadHoaDon();
