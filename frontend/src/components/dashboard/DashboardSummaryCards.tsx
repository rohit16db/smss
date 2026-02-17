import React from 'react';
import type { DashboardSummaryCard } from '../../types/dashboard';
import { TrendingUp, TrendingDown, MoreVertical } from 'lucide-react';

interface DashboardSummaryCardsProps {
  cards: DashboardSummaryCard[];
  isLoading?: boolean;
}

export const DashboardSummaryCards: React.FC<DashboardSummaryCardsProps> = ({ cards, isLoading = false }) => {
  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {[...Array(6)].map((_, i) => (
          <div key={i} className="bg-white rounded-lg shadow-md p-6 animate-pulse">
            <div className="h-4 bg-gray-200 rounded w-2/3 mb-4"></div>
            <div className="h-8 bg-gray-200 rounded w-1/2 mb-2"></div>
            <div className="h-3 bg-gray-200 rounded w-1/3"></div>
          </div>
        ))}
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {cards.map((card, index) => (
        <div key={index} className="bg-white rounded-lg shadow-md hover:shadow-lg transition-shadow p-6">
          {/* Header */}
          <div className="flex items-start justify-between mb-4">
            <h3 className="text-gray-600 text-sm font-medium">{card.title}</h3>
            <button className="text-gray-400 hover:text-gray-600">
              <MoreVertical className="w-4 h-4" />
            </button>
          </div>

          {/* Main Value */}
          <div className="mb-4">
            <div className="text-3xl font-bold text-gray-900">
              {typeof card.value === 'number'
                ? card.value > 1000
                  ? (card.value / 1000).toFixed(1) + 'k'
                  : card.value.toFixed(card.unit === '%' ? 1 : 0)
                : '--'}
              {card.unit && <span className="text-lg ml-2 text-gray-600">{card.unit}</span>}
            </div>
          </div>

          {/* Trend */}
          <div className="flex items-center justify-between">
            {typeof card.percentageChange === 'number' && !Number.isNaN(card.percentageChange) && (
              <div className="flex items-center gap-2">
                {card.trendDirection === 'up' && (
                  <TrendingUp className="w-4 h-4 text-green-500" />
                )}
                {card.trendDirection === 'down' && (
                  <TrendingDown className="w-4 h-4 text-red-500" />
                )}
                <span
                  className={`text-sm font-medium ${
                    card.trendDirection === 'up'
                      ? 'text-green-600'
                      : card.trendDirection === 'down'
                      ? 'text-red-600'
                      : 'text-gray-600'
                  }`}
                >
                  {card.percentageChange > 0 ? '+' : ''}
                  {card.percentageChange.toFixed(1)}%
                </span>
              </div>
            )}
            {card.trendDirection && (card.percentageChange === undefined || card.percentageChange === null) && (
              <div className="flex items-center gap-2">
                {card.trendDirection === 'up' && (
                  <div className="flex items-center gap-1 text-green-600 text-sm">
                    <TrendingUp className="w-4 h-4" />
                    <span>Trending Up</span>
                  </div>
                )}
                {card.trendDirection === 'down' && (
                  <div className="flex items-center gap-1 text-red-600 text-sm">
                    <TrendingDown className="w-4 h-4" />
                    <span>Trending Down</span>
                  </div>
                )}
                {card.trendDirection === 'stable' && (
                  <span className="text-gray-500 text-sm">Stable</span>
                )}
              </div>
            )}
          </div>
        </div>
      ))}
    </div>
  );
};
