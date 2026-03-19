document.addEventListener('DOMContentLoaded', function() {
    // Select All Checkbox
    document.getElementById('selectAllCheckbox').addEventListener('change', function() {
        const isChecked = this.checked;
        document.querySelectorAll('.select-checkbox').forEach(checkbox => {
            checkbox.checked = isChecked;
        });
    });

    // Add Product Row for Batch Insert
    document.getElementById('addProductRow').addEventListener('click', function() {
        const container = document.getElementById('productsContainer');
        const newRow = document.createElement('div');
        newRow.className = 'product-row mb-3';
        newRow.innerHTML = `
            <div class="row">
                <div class="col-md-4"><input type="text" class="form-control product-name" placeholder="Product Name"></div>
                <div class="col-md-2"><input type="number" class="form-control product-price" placeholder="Price" step="0.01"></div>
                <div class="col-md-2"><input type="number" class="form-control product-quantity" placeholder="Qty"></div>
                <div class="col-md-4"><input type="text" class="form-control product-category" placeholder="Category"></div>
            </div>
        `;
        container.appendChild(newRow);
    });

    // Batch Insert Submit
    document.getElementById('submitBatchInsert').addEventListener('click', async function() {
        const products = [];
        document.querySelectorAll('.product-row').forEach(row => {
            const name = row.querySelector('.product-name').value;
            const price = parseFloat(row.querySelector('.product-price').value);
            const quantity = parseInt(row.querySelector('.product-quantity').value);
            const category = row.querySelector('.product-category').value;

            if (name && price && quantity) {
                products.push({
                    id: 0,
                    name: name,
                    price: price,
                    quantity: quantity,
                    category: category || null,
                    description: null
                });
            }
        });

        if (products.length === 0) {
            alert('Please fill in at least one product');
            return;
        }

        try {
            const response = await fetch('/products/batch-insert', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ products: products })
            });

            const data = await response.json();
            
            if (response.ok) {
                alert(`Successfully inserted ${data.count} products!`);
                location.reload();
            } else {
                alert(`Error: ${data.message}`);
            }
        } catch (error) {
            alert('Error inserting products: ' + error.message);
        }
    });

    // Batch Update Modal Population
    document.getElementById('batchUpdateBtn').addEventListener('click', function() {
        const selectedIds = getSelectedProductIds();
        
        if (selectedIds.length === 0) {
            alert('Please select at least one product to update');
            const batchUpdateModal = bootstrap.Modal.getInstance(document.getElementById('batchUpdateModal'));
            if (batchUpdateModal) batchUpdateModal.hide();
            return;
        }

        populateUpdateModal(selectedIds);
    });

    function populateUpdateModal(productIds) {
        const container = document.getElementById('updateProductsContainer');
        container.innerHTML = '';

        const rows = document.querySelectorAll('tbody tr');
        
        rows.forEach(row => {
            const checkbox = row.querySelector('.select-checkbox');
            if (checkbox && checkbox.checked) {
                const id = checkbox.dataset.id;
                const name = row.cells[2].textContent;
                const category = row.cells[3].textContent;
                const price = parseFloat(row.cells[4].textContent.replace('$', ''));
                const quantity = parseInt(row.cells[5].textContent);

                const updateRow = document.createElement('div');
                updateRow.className = 'update-row mb-3 p-3 border rounded';
                updateRow.innerHTML = `
                    <h6>Product ID: ${id}</h6>
                    <div class="row">
                        <div class="col-md-4">
                            <label class="form-label">Name</label>
                            <input type="text" class="form-control update-name" value="${name}" data-id="${id}">
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Price</label>
                            <input type="number" class="form-control update-price" value="${price}" step="0.01" data-id="${id}">
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Qty</label>
                            <input type="number" class="form-control update-quantity" value="${quantity}" data-id="${id}">
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Category</label>
                            <input type="text" class="form-control update-category" value="${category === '-' ? '' : category}" data-id="${id}">
                        </div>
                    </div>
                `;
                container.appendChild(updateRow);
            }
        });
    }

    // Batch Update Submit
    document.getElementById('submitBatchUpdate').addEventListener('click', async function() {
        const products = [];
        document.querySelectorAll('.update-row').forEach(row => {
            const id = parseInt(row.querySelector('.update-name').dataset.id);
            const name = row.querySelector('.update-name').value;
            const price = parseFloat(row.querySelector('.update-price').value);
            const quantity = parseInt(row.querySelector('.update-quantity').value);
            const category = row.querySelector('.update-category').value;

            products.push({
                id: id,
                name: name,
                price: price,
                quantity: quantity,
                category: category || null,
                description: null
            });
        });

        try {
            const response = await fetch('/products/batch-update', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ products: products })
            });

            const data = await response.json();
            
            if (response.ok) {
                alert(`Successfully updated ${data.count} products!`);
                location.reload();
            } else {
                alert(`Error: ${data.message}`);
            }
        } catch (error) {
            alert('Error updating products: ' + error.message);
        }
    });

    // Batch Delete Submit
    document.getElementById('batchDeleteBtn').addEventListener('click', async function() {
        const selectedIds = getSelectedProductIds();

        if (selectedIds.length === 0) {
            alert('Please select at least one product to delete');
            return;
        }

        if (!confirm(`Are you sure you want to delete ${selectedIds.length} product(s)?`)) {
            return;
        }

        try {
            const response = await fetch('/products/batch-delete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ productIds: selectedIds })
            });

            const data = await response.json();
            
            if (response.ok) {
                alert(`Successfully deleted ${data.count} products!`);
                location.reload();
            } else {
                alert(`Error: ${data.message}`);
            }
        } catch (error) {
            alert('Error deleting products: ' + error.message);
        }
    });

    function getSelectedProductIds() {
        const selectedIds = [];
        document.querySelectorAll('.select-checkbox:checked').forEach(checkbox => {
            const id = checkbox.dataset.id;
            if (id) {
                selectedIds.push(parseInt(id));
            }
        });
        return selectedIds;
    }
});
