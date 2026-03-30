
    function confirmDelete(id, title) {
            if (confirm(`Bạn có chắc chắn muốn xóa phòng trọ "${title}"? Hành động này không thể hoàn tác.`)) {
        document.getElementById('deleteRoomId').value = id;
    document.getElementById('deleteForm').submit();
            }
