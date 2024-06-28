'use client'
import React, { useState, useEffect } from 'react'

import axios from 'axios'
import Button from '@mui/material/Button'
import './Confirm.css'

function Confirm() {
  const [isButtonDisabled, setIsButtonDisabled] = useState(true)
  const [countdown, setCountdown] = useState(60)
  const [email, setEmail] = useState('')

  useEffect(() => {
    const storedEmail = localStorage.getItem('email')

    if (storedEmail) {
      setEmail(storedEmail)
    }

    console.log(storedEmail)
  }, [])
  console.log(email)
  useEffect(() => {
    if (countdown > 0) {
      const timer = setInterval(() => {
        setCountdown(prevCountdown => prevCountdown - 1)
      }, 1000)

      return () => clearInterval(timer)
    } else {
      setIsButtonDisabled(false)
    }
  }, [countdown])

  const handleResendEmail = async () => {
    try {
      await axios.post(`https://localhost:7288/api/Account/ResendConfirmEmail?Email=${email}`)
      setIsButtonDisabled(true)
      setCountdown(60)
    } catch (error) {
      console.error('Error resending confirmation email:', error)
    }
  }

  return (
    <>
      <div className='container'>
        <div className='confirm'>
          <h1>Please Check Your Email Box</h1>
          <Button variant='contained' type='submit' onClick={handleResendEmail} disabled={isButtonDisabled}>
            {isButtonDisabled ? `Resend Email (${countdown})` : 'Resend Email'}
          </Button>
        </div>
      </div>
    </>
  )
}

export default Confirm
