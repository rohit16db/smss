/**
 * Component Unit Tests: LoadingSkeleton and EmptyState
 * Verifies UI components render correctly
 */
import { describe, it, expect } from 'vitest';
import { render, screen } from '../test/test-utils';
import { LoadingSkeleton } from '../components/common/LoadingSkeleton';
import { EmptyState, NoDataIcon } from '../components/common/EmptyState';

describe('LoadingSkeleton Component', () => {
  it('should render table skeleton by default', () => {
    render(<LoadingSkeleton />);
    
    const tables = document.querySelectorAll('table');
    expect(tables.length).toBeGreaterThan(0);
  });

  it('should render card skeleton when type is card', () => {
    render(<LoadingSkeleton type="card" rows={3} />);
    
    const cards = document.querySelectorAll('.animate-pulse');
    expect(cards.length).toBeGreaterThan(0);
  });

  it('should render form skeleton when type is form', () => {
    render(<LoadingSkeleton type="form" rows={4} />);
    
    const forms = document.querySelectorAll('.animate-pulse');
    expect(forms.length).toBeGreaterThan(0);
  });

  it('should render specified number of rows', () => {
    const rows = 5;
    render(<LoadingSkeleton rows={rows} type="table" />);
    
    const tableRows = document.querySelectorAll('tbody tr');
    expect(tableRows.length).toBe(rows);
  });
});

describe('EmptyState Component', () => {
  it('should render title', () => {
    render(<EmptyState title="No data found" />);
    
    expect(screen.queryByText('No data found')).toBeTruthy();
  });

  it('should render description when provided', () => {
    render(
      <EmptyState 
        title="No data" 
        description="Try adjusting your search" 
      />
    );
    
    expect(screen.queryByText('Try adjusting your search')).toBeTruthy();
  });

  it('should render action button when provided', () => {
    const onClick = () => {};
    render(
      <EmptyState 
        title="No data" 
        action={{ label: 'Add Item', onClick }}
      />
    );
    
    expect(screen.queryByRole('button', { name: 'Add Item' })).toBeTruthy();
  });

  it('should render custom icon when provided', () => {
    render(
      <EmptyState 
        title="No data" 
        icon={<NoDataIcon />}
      />
    );
    
    const svg = document.querySelector('svg');
    expect(svg).toBeTruthy();
  });

  it('should not render description when not provided', () => {
    render(<EmptyState title="No data" />);
    
    const paragraphs = document.querySelectorAll('p');
    expect(paragraphs.length).toBe(0);
  });

  it('should not render action button when not provided', () => {
    render(<EmptyState title="No data" />);
    
    const buttons = screen.queryAllByRole('button');
    expect(buttons.length).toBe(0);
  });
});

describe('NoDataIcon Component', () => {
  it('should render SVG icon', () => {
    const { container } = render(<NoDataIcon />);
    
    const svg = container.querySelector('svg');
    expect(svg).toBeTruthy();
    expect(svg?.classList.contains('w-16')).toBe(true);
    expect(svg?.classList.contains('h-16')).toBe(true);
  });

  it('should have aria-hidden attribute', () => {
    const { container } = render(<NoDataIcon />);
    
    const svg = container.querySelector('svg');
    expect(svg?.getAttribute('aria-hidden')).toBe('true');
  });
});
