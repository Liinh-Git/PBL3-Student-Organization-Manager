/**
 * Pagination.jsx - Pagination component
 * 
 * Phase 3C-4A: Foundation skeleton only
 * 
 * TODO Phase 3C-4B/4C Implementation:
 * - Add page number buttons
 * - Add prev/next buttons
 * - Add first/last buttons
 * - Handle page size selection
 * - Display total count and current range
 * 
 * Usage:
 *   <Pagination
 *     currentPage={1}
 *     totalPages={10}
 *     onPageChange={handlePageChange}
 *   />
 */

function Pagination({ 
  currentPage = 1,
  totalPages = 1,
  onPageChange,
  pageSize = 10,
  totalCount = 0
}) {
  const handlePrevious = () => {
    if (currentPage > 1) {
      onPageChange(currentPage - 1);
    }
  };

  const handleNext = () => {
    if (currentPage < totalPages) {
      onPageChange(currentPage + 1);
    }
  };

  return (
    <div className="pagination">
      <button 
        onClick={handlePrevious} 
        disabled={currentPage === 1}
        className="pagination-button"
      >
        Previous
      </button>
      
      <span className="pagination-info">
        Page {currentPage} of {totalPages}
      </span>
      
      <button 
        onClick={handleNext} 
        disabled={currentPage === totalPages}
        className="pagination-button"
      >
        Next
      </button>
    </div>
  );
}

export default Pagination;
