import React from 'react';
import { TrendingUp, TrendingDown } from 'lucide-react';
import type { LucideIcon } from 'lucide-react';

interface FeeCollectionSummaryCardProps {
  title: string;
  amount: number;
  icon?: LucideIcon;
  trend?: number;
  trendLabel?: string;
  textColor?: string;
  count?: number;
  countLabel?: string;
}

/**
 * Card component for displaying fee collection metrics
 */
export const FeeCollectionSummaryCard: React.FC<FeeCollectionSummaryCardProps> = ({
  title,
  amount,
  icon: Icon,
  trend,
  trendLabel = 'Change',
  textColor = 'text-blue-600',
  count,
  countLabel,
}) => {
  const isTrendPositive = trend !== undefined && trend > 0;
  const isTrendNegative = trend !== undefined && trend < 0;

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <p className="text-sm font-medium text-gray-600">{title}</p>
          <div className="flex items-baseline gap-2 mt-2">
            <h3 className={`text-3xl font-bold ${textColor}`}>
              ₹{amount.toLocaleString('en-IN', { maximumFractionDigits: 0 })}
            </h3>
          </div>

          {/* Trend indicator */}
          {trend !== undefined && (
            <div className="flex items-center gap-1 mt-2">
              {isTrendPositive ? (
                <TrendingUp className="w-4 h-4 text-green-600" />
              ) : isTrendNegative ? (
                <TrendingDown className="w-4 h-4 text-red-600" />
              ) : null}
              <span
                className={`text-sm font-medium ${
                  isTrendPositive ? 'text-green-600' : isTrendNegative ? 'text-red-600' : 'text-gray-600'
                }`}
              >
                {isTrendPositive && '+'}
                {trend.toFixed(1)}% {trendLabel}
              </span>
            </div>
          )}

          {/* Count indicator */}
          {count !== undefined && countLabel && (
            <p className="text-sm text-gray-500 mt-1">
              {count} {countLabel}
            </p>
          )}
        </div>

        {Icon && (
          <div className={`p-3 rounded-lg bg-opacity-10 ${textColor}`}>
            <Icon className={`w-6 h-6 ${textColor}`} />
          </div>
        )}
      </div>
    </div>
  );
};
