import ConfPassword from '../../../../views/ConfPassword'

// Server Action Imports
import { getServerMode } from '@core/utils/serverHelpers'

export const metadata = {
  title: 'ConfirmPassword',
  description: 'Confirm your Password'
}

const ConfirmPage = () => {
  // Vars
  const mode = getServerMode()

  return <ConfPassword mode={mode} />
}

export default ConfirmPage
